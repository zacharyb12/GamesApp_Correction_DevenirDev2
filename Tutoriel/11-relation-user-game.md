# Chapitre 11 – Relier un jeu à son utilisateur

## 🎯 Objectif du chapitre

Ajouter une relation **1-N** entre `User` et `Game` : un utilisateur possède plusieurs jeux, un jeu a **au plus** un propriétaire. À la fin tu auras une colonne `UserId` (nullable) dans la table `Games` et tu sauras persister l'id du créateur lors de la création d'un jeu.

---

## 11.1 Ajouter les propriétés de navigation

### Côté Game

Ouvre `Models/GameModels/Game.cs` et ajoute :

```csharp
using GamesApp.Models.UserModels;

namespace GamesApp.Models.GameModels
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Editor { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }

        // relation vers User (clé étrangère Identity = string)
        public string? UserId { get; set; }
        public User? User { get; set; }
    }
}
```

- `UserId` est de type `string?` parce que la PK d'`IdentityUser` est un GUID sérialisé en string.
- Les deux sont **nullables** : on accepte les jeux "orphelins" (sans propriétaire). Ça nous servira au cas où on supprime un user sans supprimer ses jeux.

### Côté User

Ouvre `Models/UserModels/User.cs` et ajoute la collection :

```csharp
using Microsoft.AspNetCore.Identity;

namespace GamesApp.Models.UserModels
{
    public class User : IdentityUser
    {
        public string Prenom { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<GameModels.Game> MyGames { get; set; } = new();
    }
}
```

Cette collection est la face "N" de la relation. Initialisée à `new List<Game>()` pour éviter les `NullReferenceException`.

---

## 11.2 Déclarer la relation en Fluent API

Ouvre `Data/Configurations/GameConfiguration.cs` et ajoute **à la fin** de `Configure` :

```csharp
// Relation vers User (optionnelle) — met la FK UserId et conserve les jeux si user supprimé
builder.HasOne(g => g.User)
       .WithMany(u => u.MyGames)
       .HasForeignKey(g => g.UserId)
       .OnDelete(DeleteBehavior.SetNull);
```

Traduction :

- `HasOne(g => g.User)` + `WithMany(u => u.MyGames)` → relation 1-N.
- `HasForeignKey(g => g.UserId)` → la FK s'appelle `UserId`.
- `OnDelete(DeleteBehavior.SetNull)` → si on supprime un utilisateur, ses jeux restent mais leur `UserId` devient `NULL`. C'est exactement ce qu'on veut pour respecter l'historique.

---

## 11.3 Migration

Ouvre la PMC :

```powershell
Add-Migration add-game-to-user
```

Le fichier généré doit créer une colonne `UserId nvarchar(450) NULL` dans `dbo.Games` + une FK vers `AspNetUsers(Id)` avec `ON DELETE SET NULL`.

```powershell
Update-Database
```

Vérifie dans l'Explorateur SQL : la colonne `UserId` existe dans `Games` et une contrainte FK est créée.

---

## 11.4 Stocker l'UserId lors de la création d'un jeu

Le problème : quand on crée un `Game` via le formulaire actuel (chap. 5), on ne sauvegarde pas qui l'a créé. Il faut passer le `userId` du user connecté au repository.

### 11.4.1 Mettre à jour l'interface IGameRepository

```csharp
Task<bool> Create(CreateGameDTOs newGame, string userId);
Task<IEnumerable<Game>> GetGamesByUserId(string userId);
```

On ajoute `string userId` au Create, et on ajoute une méthode `GetGamesByUserId` pour le chapitre 13.

### 11.4.2 Implémenter dans GameRepository

Remplace la méthode `Create` :

```csharp
public async Task<bool> Create(CreateGameDTOs newGame, string userId)
{
    Game game = new()
    {
        Name = newGame.Name,
        Description = newGame.Description,
        Editor = newGame.Editor,
        Quantity = newGame.Quantity,
        Price = newGame.Price,
        ImageUrl = newGame.ImageUrl,
        UserId = userId
    };

    _context.Games.Add(game);
    await _context.SaveChangesAsync();

    if (game.Id == 0)
    {
        return false;
    }

    return true;
}
```

Et ajoute la méthode `GetGamesByUserId` (utilisée au chapitre 13) :

```csharp
public async Task<IEnumerable<Game>> GetGamesByUserId(string userId)
{
    return await _context.Games
        .Where(g => g.UserId == userId)
        .ToListAsync();
}
```

### 11.4.3 Adapter le GameController.Create

Dans `GameController.cs`, modifie le POST `Create` pour récupérer l'id du user connecté :

```csharp
using System.Security.Claims;

// POST: GameController/Create
[HttpPost]
[Authorize]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Create(CreateGameDTOs newGame)
{
    try
    {
        if (!ModelState.IsValid)
        {
            return View(newGame);
        }

        // Récupérer l'ID de l'utilisateur connecté
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        bool created = await _repository.Create(newGame, userId!);

        if (!created)
        {
            return View(newGame);
        }

        return RedirectToAction("Index");
    }
    catch
    {
        return View(newGame);
    }
}
```

Décodage de `User.FindFirstValue(ClaimTypes.NameIdentifier)` :

- `User` (dans un controller) est le `ClaimsPrincipal` de l'utilisateur courant.
- `FindFirstValue(ClaimTypes.NameIdentifier)` récupère la première claim de type "identifiant", qui est l'`Id` de l'utilisateur (string).
- Le `!` après `userId` dit au compilo "crois-moi, c'est pas null" (on est dans une action `[Authorize]`, donc c'est forcément connecté).

---

## 11.5 Tester

1. Connecte-toi.
2. Crée un nouveau jeu via `/Game/Create`.
3. Va regarder la table `Games` dans la BDD → la colonne `UserId` du nouveau jeu doit contenir l'id de ton utilisateur.
4. Les anciens jeux (créés avant ce chapitre) ont `UserId = NULL` — c'est normal.

---

## ✅ Point de contrôle

- [ ] La colonne `UserId` existe dans `dbo.Games`.
- [ ] La FK vers `AspNetUsers(Id)` est configurée avec `ON DELETE SET NULL`.
- [ ] `IGameRepository.Create` accepte maintenant un `userId`.
- [ ] `GameController.Create` (POST) passe bien `userId` au repository.
- [ ] Un nouveau jeu créé a bien son `UserId` en base.
- [ ] La méthode `GetGamesByUserId` existe dans le repository (on l'utilisera au chap. 13).

Direction → [Chapitre 12 : Autoriser seulement le propriétaire ou l'admin](./12-autorisation-proprietaire-ou-admin.md).
