# Chapitre 5 – Créer un jeu (Create)

## 🎯 Objectif du chapitre

Permettre à l'utilisateur d'ajouter un nouveau jeu via un formulaire. On introduit ici le **DTO** (Data Transfer Object), un objet plus restreint que l'entité `Game`, dédié au formulaire.

---

## 5.1 Créer le DTO CreateGameDTOs

Dans `Models/GameModels/`, nouvelle classe **CreateGameDTOs.cs** :

```csharp
namespace GamesApp.Models.GameModels
{
    public class CreateGameDTOs
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Editor { get; set; }

        public decimal Price { get; set; }

        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
    }
}
```

Pourquoi un DTO plutôt que `Game` directement ?

- On ne veut **pas** que l'utilisateur puisse envoyer `Id` ou `CreatedAt` depuis le formulaire (risque d'over-posting).
- On garde la classe `Game` "pure" pour la BDD, et la classe `CreateGameDTOs` "pure" pour le web.

---

## 5.2 Adapter le repository

On va modifier `IGameRepository.Create` et `GameRepository.Create` pour qu'ils prennent un `CreateGameDTOs` au lieu d'un `Game`.

Dans **IGameRepository.cs** :

```csharp
Task<bool> Create(CreateGameDTOs newGame);
```

Dans **GameRepository.cs**, remplace la méthode `Create` :

```csharp
public async Task<bool> Create(CreateGameDTOs newGame)
{
    Game game = new()
    {
        Name = newGame.Name,
        Description = newGame.Description,
        Editor = newGame.Editor,
        Quantity = newGame.Quantity,
        Price = newGame.Price,
        ImageUrl = newGame.ImageUrl
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

C'est le repository qui transforme le DTO en entité.

---

## 5.3 Ajouter les 2 actions Create dans GameController

Dans `GameController.cs`, ajoute :

```csharp
// GET: GameController/Create
public ActionResult Create()
{
    return View(new CreateGameDTOs());
}

// POST: GameController/Create
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Create(CreateGameDTOs newGame)
{
    try
    {
        if (!ModelState.IsValid)
        {
            return View(newGame);
        }

        bool created = await _repository.Create(newGame);

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

### Explication

- **2 méthodes du même nom** : c'est le pattern classique ASP.NET MVC. La GET affiche le formulaire vide, la POST traite la soumission.
- `[ValidateAntiForgeryToken]` : protège contre les attaques CSRF. Le formulaire doit envoyer le token (généré automatiquement par le Tag Helper `<form>`).
- `ModelState.IsValid` : vérifie que les données reçues sont valides (utile quand on mettra des `[Required]` sur le DTO).
- `RedirectToAction("Index")` après succès → pattern **Post-Redirect-Get** : évite la double soumission si l'utilisateur rafraîchit.

---

## 5.4 Créer la vue Create

Dans `Views/Game/`, nouvelle vue **Create.cshtml** :

```cshtml
@using GamesApp.Models.GameModels
@model CreateGameDTOs

<h2>Ajouter un Jeux</h2>

<form method="post" asp-action="Create">
    <div>
        <label asp-for="Name"></label>
        <input asp-for="Name" type="text" />
    </div>

    <div>
        <label asp-for="Description"></label>
        <input asp-for="Description" type="text" />
    </div>

    <div>
        <label asp-for="Editor"></label>
        <input asp-for="Editor" type="text" />
    </div>

    <div>
        <label asp-for="Quantity"></label>
        <input asp-for="Quantity" type="number" />
    </div>

    <div>
        <label asp-for="ImageUrl"></label>
        <input asp-for="ImageUrl" type="text" />
    </div>

    <div>
        <label asp-for="Price"></label>
        <input asp-for="Price" type="number" step="0.1" />
    </div>

    <input type="submit" value="Create" />
</form>
```

Points clés :

- `<form method="post" asp-action="Create">` → le Tag Helper ajoute **automatiquement** le champ caché `__RequestVerificationToken` (anti-CSRF) et l'URL cible.
- `<input asp-for="Name" />` → génère `name="Name"` et `id="Name"` qui matcheront le binding côté POST.

---

## 5.5 Ajouter le lien dans la navbar

Ouvre `Views/Shared/_Layout.cshtml` et ajoute dans la `<ul class="navbar-nav flex-grow-1">` :

```html
<li class="nav-item">
    <a class="nav-link text-dark" asp-controller="Game" asp-action="Create">Ajouter un jeu</a>
</li>
```

> 💡 Pour l'instant tout le monde peut créer. On protégera ça avec `[Authorize]` au chapitre 10.

---

## ✅ Point de contrôle

- [ ] `/Game/Create` affiche le formulaire.
- [ ] Soumettre le formulaire avec des valeurs valides crée un jeu et redirige sur `/Game`.
- [ ] Le nouveau jeu apparaît bien dans la liste.
- [ ] Le bouton "Ajouter un jeu" dans la navbar fonctionne.

Direction → [Chapitre 6 : Modifier un jeu (Edit)](./06-modifier-un-jeu.md).
