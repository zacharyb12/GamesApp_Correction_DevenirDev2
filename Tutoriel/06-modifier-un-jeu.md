# Chapitre 6 – Modifier un jeu (Edit)

## 🎯 Objectif du chapitre

Créer un formulaire de modification pour un jeu existant. Comme pour le Create, on passe par un DTO dédié.

---

## 6.1 Créer le DTO UpdateGameDTOs

Dans `Models/GameModels/`, nouvelle classe **UpdateGameDTOs.cs** :

```csharp
namespace GamesApp.Models.GameModels
{
    public class UpdateGameDTOs
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
```

Différences avec `CreateGameDTOs` :

- On a `Id` (obligatoire pour savoir quel jeu mettre à jour).
- On n'a **pas** `Editor` : on a décidé qu'un utilisateur ne peut pas changer l'éditeur d'un jeu après création.

> 🎓 Point pédagogique à expliquer : la **liste des champs modifiables** doit être explicite, pas implicite. C'est le DTO qui définit ce qui peut bouger.

---

## 6.2 Adapter le repository

Dans **IGameRepository.cs** :

```csharp
Task<bool> Update(UpdateGameDTOs updatedGame);
```

Dans **GameRepository.cs**, remplace la méthode `Update` :

```csharp
public async Task<bool> Update(UpdateGameDTOs updatedGame)
{
    Game? game = await _context.Games.FirstOrDefaultAsync(g => g.Id == updatedGame.Id);
    if (game == null)
    {
        return false;
    }
    game.Name = updatedGame.Name;
    game.Description = updatedGame.Description;
    game.Quantity = updatedGame.Quantity;
    game.ImageUrl = updatedGame.ImageUrl;
    game.Price = updatedGame.Price;

    await _context.SaveChangesAsync();

    return true;
}
```

> ⚠️ On **ne** fait **pas** `game.Editor = updatedGame.Editor` : le DTO ne l'expose pas.

---

## 6.3 Les 2 actions Edit du controller

Dans `GameController.cs`, ajoute :

```csharp
// GET: GameController/Edit/5
public async Task<ActionResult> Edit(int id)
{
    Game? game = await _repository.GetById(id);

    if (game == null)
    {
        return RedirectToAction("Index");
    }

    UpdateGameDTOs gameToUpdate = new()
    {
        Id = game.Id,
        Name = game.Name,
        Description = game.Description,
        Quantity = game.Quantity,
        ImageUrl = game.ImageUrl,
        Price = game.Price
    };

    return View(gameToUpdate);
}

// POST: GameController/Edit/5
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Edit(UpdateGameDTOs updatedGame)
{
    try
    {
        bool response = await _repository.Update(updatedGame);

        if (!response)
        {
            return View(updatedGame);
        }
        return RedirectToAction("Index");
    }
    catch
    {
        return View(updatedGame);
    }
}
```

Ce qui se passe :

- **GET** : on charge le jeu depuis la BDD, on le transforme en DTO (pour pré-remplir le formulaire), on envoie ça à la vue.
- **POST** : le binding reconstitue un `UpdateGameDTOs` à partir des champs du formulaire, on appelle le repository, redirect si OK, vue réaffichée si KO.

---

## 6.4 Créer la vue Edit

Dans `Views/Game/`, nouvelle vue **Edit.cshtml** :

```cshtml
@model GamesApp.Models.GameModels.UpdateGameDTOs

<h2 class="text-center mt-5 mb-5">Modification de : @Model.Name</h2>

<form class="card w-75 mx-auto text-center" method="post" asp-action="Edit">
    <div class="p-2 m-3">
        <label asp-for="Name" class="form-label"></label>
        <input asp-for="Name" type="text" class="form-control w-50 mx-auto" />
    </div>

    <div class="p-2 m-3">
        <label asp-for="Description" class="form-label"></label>
        <input asp-for="Description" type="text" class="form-control w-50 mx-auto" />
    </div>

    <div class="p-2 m-3">
        <label asp-for="Quantity" class="form-label"></label>
        <input asp-for="Quantity" type="number" class="form-control w-50 mx-auto" />
    </div>

    <div class="p-2 m-3">
        <label asp-for="ImageUrl" class="form-label"></label>
        <input asp-for="ImageUrl" type="text" class="form-control w-50 mx-auto" />
    </div>

    <div class="p-2 m-3">
        <label asp-for="Price" class="form-label"></label>
        <input asp-for="Price" type="number" step="0.1" class="form-control w-50 mx-auto" />
    </div>

    <div class="d-flex justify-content-evenly mb-5">
        <input type="submit" value="Update" class="btn btn-outline-success w-25" />
        <a asp-controller="Game" asp-action="Delete" asp-route-id="@Model.Id" class="btn btn-outline-danger w-25">Delete</a>
    </div>
</form>
```

> 🔎 Question piège pour tes stagiaires : où est le champ `Id` ? Il n'est **pas** affiché, mais il est quand même envoyé dans la requête POST.
>
> **Réponse** : les Tag Helpers ne rendent pas de champ caché tout seuls. Ici ça marche parce que l'URL est `/Game/Edit/5`, et le routage ASP.NET met `id=5` dans la route qui est ensuite bindé au `Id` du DTO. Si jamais tu changes de route ou si tu veux être 100% safe, ajoute `<input type="hidden" asp-for="Id" />` en haut du formulaire.

---

## 6.5 Ajouter un lien Edit depuis Details

Ouvre `Views/Game/Details.cshtml` et ajoute **dans la `<div class="card ...">`** après l'affichage des données :

```cshtml
<a asp-controller="Game" asp-action="Edit" asp-route-id="@Model.Id" class="btn btn-outline-secondary mx-auto mb-3">Edit</a>
```

---

## ✅ Point de contrôle

- [ ] `/Game/Edit/1` affiche un formulaire pré-rempli avec les infos du jeu 1.
- [ ] Modifier puis soumettre met bien à jour la BDD et redirige sur `/Game`.
- [ ] Le champ `Editor` du jeu reste **inchangé** après l'Edit.
- [ ] Le bouton "Edit" dans Details navigue bien vers le formulaire.

Direction → [Chapitre 7 : Supprimer un jeu (Delete)](./07-supprimer-un-jeu.md).
