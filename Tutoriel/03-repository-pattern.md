# Chapitre 3 – Le Repository Pattern

## 🎯 Objectif du chapitre

Mettre en place un **repository** pour la classe `Game` : une interface `IGameRepository` + son implémentation `GameRepository`. Le controller ne parlera **jamais** directement au `DbContext`, seulement au repository.

## 3.1 Pourquoi un repository ?

Trois raisons :

1. **Séparation des responsabilités** : le controller gère le HTTP, le repository gère la BDD.
2. **Testabilité** : on peut mocker `IGameRepository` dans des tests unitaires.
3. **Réutilisabilité** : la même méthode `GetById` peut être appelée depuis plusieurs endroits.

---

## 3.2 Créer l'interface IGameRepository

Clic droit sur le projet → nouveau dossier **Repositories**.
Dans `Repositories/`, clic droit → `Ajouter → Interface...` → **IGameRepository.cs**.

```csharp
using GamesApp.Models.GameModels;

namespace GamesApp.Repositories
{
    public interface IGameRepository
    {
        Task<IEnumerable<Game>> GetGames();
        Task<Game?> GetById(int id);
        Task<bool> Create(Game game);
        Task<bool> Update(Game game);
        Task<bool> Delete(int id);
    }
}
```

> 💡 Pour l'instant `Create` prend un `Game`. On le remplacera par un `CreateGameDTOs` au chapitre 5. Ça évite d'exposer directement l'entité dans les formulaires (bonne pratique sécurité → pas d'over-posting).

---

## 3.3 Créer l'implémentation GameRepository

Dans `Repositories/`, clic droit → `Ajouter → Classe...` → **GameRepository.cs**.

```csharp
using GamesApp.Data;
using GamesApp.Models.GameModels;
using Microsoft.EntityFrameworkCore;

namespace GamesApp.Repositories
{
    public class GameRepository(GameContext _context) : IGameRepository
    {
        public async Task<bool> Create(Game game)
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            if (game.Id == 0)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            Game? game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
            {
                return false;
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Game?> GetById(int id)
        {
            Game? game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
            {
                return null;
            }
            return game;
        }

        public async Task<IEnumerable<Game>> GetGames()
        {
            return await _context.Games.ToListAsync();
        }

        public async Task<bool> Update(Game game)
        {
            Game? existing = await _context.Games.FirstOrDefaultAsync(g => g.Id == game.Id);
            if (existing == null)
            {
                return false;
            }
            existing.Name = game.Name;
            existing.Description = game.Description;
            existing.Quantity = game.Quantity;
            existing.ImageUrl = game.ImageUrl;
            existing.Price = game.Price;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
```

### À noter sur ce code

- **Constructeur primaire** : `public class GameRepository(GameContext _context) : IGameRepository` — syntaxe C# 12+. Le paramètre `_context` est directement utilisable dans les méthodes. Pas besoin de champ privé ni de constructeur.
- Les méthodes renvoient `bool` (succès/échec) plutôt que lever des exceptions. C'est volontaire pour garder le code simple côté controller.
- `Update` fait un **UPDATE partiel** : on recharge l'entité existante et on écrase uniquement les champs modifiables (`Name`, `Description`, etc.). Ça évite qu'un champ oublié dans le DTO écrase la colonne en base avec `null`.

---

## 3.4 Enregistrer le repository dans Program.cs

Pour que l'injection de dépendances sache quoi fournir quand on demande un `IGameRepository`, ajoute dans `Program.cs`, **après** le `AddDbContext` :

```csharp
builder.Services.AddScoped<IGameRepository, GameRepository>();
```

`AddScoped` = une instance par requête HTTP. C'est le bon choix pour un repository qui utilise un DbContext (lui aussi scoped).

Ton `Program.cs` doit commencer à ressembler à :

```csharp
using GamesApp.Data;
using GamesApp.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GamesApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<GameContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("myConnection"))
            );

            builder.Services.AddScoped<IGameRepository, GameRepository>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
```

---

## ✅ Point de contrôle

- [ ] `IGameRepository.cs` existe et compile.
- [ ] `GameRepository.cs` existe, implémente bien l'interface et compile.
- [ ] La ligne `AddScoped<IGameRepository, GameRepository>()` est présente dans `Program.cs`.
- [ ] Le projet compile (Ctrl+Shift+B) sans erreur ni warning bloquant.

Direction → [Chapitre 4 : Afficher la liste et les détails d'un jeu](./04-afficher-les-jeux.md).
