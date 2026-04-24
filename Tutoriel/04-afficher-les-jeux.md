# Chapitre 4 – Afficher la liste et les détails d'un jeu

## 🎯 Objectif du chapitre

Créer le `GameController` avec deux actions en lecture seule (`Index` et `Details`), leurs vues Razor associées, et ajouter un lien *"Jeux"* dans la navbar pour y accéder.

---

## 4.1 Créer le GameController

Clic droit sur `Controllers/` → `Ajouter → Contrôleur → Contrôleur MVC - Vide` → nom **GameController**.

Remplace le contenu par :

```csharp
using GamesApp.Models.GameModels;
using GamesApp.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GamesApp.Controllers
{
    public class GameController(IGameRepository _repository) : Controller
    {
        // GET: GameController
        public async Task<ActionResult> Index()
        {
            IEnumerable<Game> games = await _repository.GetGames();

            return View(games);
        }

        // GET: GameController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            Game? game = await _repository.GetById(id);

            if (game == null)
            {
                return RedirectToAction("Index");
            }

            return View(game);
        }
    }
}
```

### Ce qu'il faut comprendre

- **Constructeur primaire** `(IGameRepository _repository)` : ASP.NET Core injecte automatiquement une instance de `GameRepository` (grâce au `AddScoped` du chapitre 3).
- `Index` récupère la liste et l'envoie à la vue.
- `Details(int id)` récupère un jeu par son id. Si `null`, on redirige vers `Index` au lieu d'afficher une page d'erreur moche.
- `return View(models)` → ASP.NET cherchera un fichier `.cshtml` dont le nom correspond à l'action dans `Views/Game/`.

---

## 4.2 Créer les vues Index et Details

### 4.2.1 Créer le dossier `Views/Game/`

Clic droit sur `Views/` → `Ajouter → Nouveau dossier` → **Game**.

### 4.2.2 La vue Index

Dans `Views/Game/`, clic droit → `Ajouter → Vue Razor - Vide` → nom **Index.cshtml**.

Contenu :

```cshtml
@model IEnumerable<GamesApp.Models.GameModels.Game>

<h2 class="text-center mt-5 mb-5">Les Jeux</h2>

<div>
    @foreach (GamesApp.Models.GameModels.Game g in Model)
    {
        <div class="d-flex">
            <div class="card w-75 rounded shadow p-3 text-center m-2">
                <h5>@g.Name</h5>
                <p>@g.Description</p>
                <p>Editeur : @g.Editor</p>
                <p>Date de sortie : @g.CreatedAt</p>
                <p>Stock : @g.Quantity</p>
                <p>@g.Price €</p>
                <p>@g.Id</p>
                <a asp-controller="Game" asp-action="Details" asp-route-id="@g.Id" class="btn btn-outline-primary">Details</a>
            </div>
            <img src="@g.ImageUrl" alt="@g.Name" width="400" class="rounded m-2"/>
        </div>
    }
</div>
```

Points clés :

- `@model IEnumerable<...>` → la vue attend une **collection** de jeux.
- `<a asp-controller="..." asp-action="..." asp-route-id="...">` → Tag Helper ASP.NET. Il génère automatiquement la bonne URL `/Game/Details/{id}`.

### 4.2.3 La vue Details

Toujours dans `Views/Game/`, crée **Details.cshtml** :

```cshtml
@model GamesApp.Models.GameModels.Game

<h2 class="text-center mt-5 mb-5">@Model.Name</h2>
<div class="container p-5">
    <div class="m-auto text-center">
        <img src="@Model.ImageUrl" alt="@Model.Name" width="800" class="rounded"/>
    </div>

    <div class="card w-50 mx-auto mt-4 p-3">
        <div class="d-flex">
            <div>
                <p class="fw-bold ms-3 me-5">Description :</p>
                <p class="fw-bold ms-3 me-5">Editeur :</p>
                <p class="fw-bold ms-3 me-5">€</p>
                <p class="fw-bold ms-3 me-5">Stock :</p>
            </div>
            <div>
                <p>@Model.Description</p>
                <p>@Model.Editor</p>
                <p>@Model.Price</p>
                <p>@Model.Quantity</p>
            </div>
        </div>
    </div>
</div>
```

Ici `@model` est au singulier car on reçoit **un seul** jeu.

---

## 4.3 Ajouter un lien dans la navbar

Ouvre `Views/Shared/_Layout.cshtml`. Dans la `<ul class="navbar-nav flex-grow-1">`, ajoute un `<li>` "Jeux" après Privacy :

```html
<li class="nav-item">
    <a class="nav-link text-dark" asp-area="" asp-controller="Game" asp-action="Index">Jeux</a>
</li>
```

---

## 4.4 Insérer des jeux de test en base

Tu peux soit coder un seeder, soit faire l'inverse (faster pour un TP) : ouvre l'Explorateur d'objets SQL Server, clic droit sur la table `Games` → `Afficher les données`, et ajoute manuellement 2-3 lignes. Exemple :

| Name | Description | Editor | Quantity | Price | ImageUrl |
|---|---|---|---|---|---|
| Hollow Knight | Metroidvania 2D | Team Cherry | 10 | 14.99 | https://images.nintendolife.com/.../hollow-knight.jpg |
| Hades | Rogue-lite mythologique | Supergiant | 5 | 24.99 | https://upload.wikimedia.org/wikipedia/en/c/cc/Hades_cover_art.jpg |

La colonne `CreatedAt` se remplit toute seule (`HasDefaultValueSql("GETDATE()")`).

---

## 4.5 Tester

Lance (F5), va sur `https://localhost:xxxx/Game`.
- Tu dois voir la liste.
- Clique sur "Details" → la page du jeu s'affiche.

---

## ✅ Point de contrôle

- [ ] `/Game` affiche la liste des jeux insérés.
- [ ] `/Game/Details/1` affiche les détails d'un jeu existant.
- [ ] `/Game/Details/99999` (id inexistant) redirige vers `/Game`.
- [ ] Le lien "Jeux" dans la navbar fonctionne.

Direction → [Chapitre 5 : Créer un jeu (Create)](./05-creer-un-jeu.md).
