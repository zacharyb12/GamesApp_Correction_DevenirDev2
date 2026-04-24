# Chapitre 13 – Page "Mes Jeux"

## 🎯 Objectif du chapitre

Ajouter une action `MyGames` sur le `GameController` qui affiche **uniquement** les jeux de l'utilisateur connecté, avec sa propre vue et un lien dans la navbar.

---

## 13.1 L'action MyGames dans le controller

Ouvre `GameController.cs` et ajoute, juste après `Details` :

```csharp
// GET: GameController/MyGames
// Affiche uniquement les jeux de l'utilisateur connecté
[Authorize]
public async Task<ActionResult> MyGames()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    IEnumerable<Game> games = await _repository.GetGamesByUserId(userId!);

    return View(games);
}
```

On utilise la méthode `GetGamesByUserId` qu'on a ajoutée au repository au chapitre 11.

Rien de compliqué ici :

- `[Authorize]` car forcément connecté ;
- on récupère l'id du user ;
- on demande au repository les jeux filtrés ;
- on envoie ça à la vue.

---

## 13.2 La vue MyGames

Dans `Views/Game/`, nouvelle vue **MyGames.cshtml** :

```cshtml
@model IEnumerable<GamesApp.Models.GameModels.Game>

<h2 class="text-center mt-5 mb-5">Mes Jeux</h2>

@if (!Model.Any())
{
    <p class="text-center">Vous n'avez encore ajouté aucun jeu.</p>
    <div class="text-center">
        <a asp-controller="Game" asp-action="Create" class="btn btn-outline-success">Ajouter un jeu</a>
    </div>
}
else
{
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
                    <div class="d-flex justify-content-evenly">
                        <a asp-controller="Game" asp-action="Details" asp-route-id="@g.Id" class="btn btn-outline-primary">Details</a>
                        <a asp-controller="Game" asp-action="Edit" asp-route-id="@g.Id" class="btn btn-outline-secondary">Edit</a>
                        <a asp-controller="Game" asp-action="Delete" asp-route-id="@g.Id" class="btn btn-outline-danger">Delete</a>
                    </div>
                </div>
                <img src="@g.ImageUrl" alt="@g.Name" width="400" class="rounded m-2" />
            </div>
        }
    </div>
}
```

Deux particularités :

- Un message convivial si la liste est vide (`!Model.Any()`).
- Les 3 boutons **Details / Edit / Delete** sont tous affichés — on est forcément propriétaire de nos propres jeux, donc la vérif serveur du chapitre 12 nous laissera passer.

---

## 13.3 Ajouter le lien "Mes Jeux" dans la navbar

Dans `Views/Shared/_Layout.cshtml`, dans le `@if (SignInManager.IsSignedIn(User))` qui entoure déjà "Ajouter un jeu", ajoute un second `<li>` juste après :

```html
<li class="nav-item">
    <a class="nav-link text-dark" asp-controller="Game" asp-action="MyGames">Mes Jeux</a>
</li>
```

Résultat : quand connecté, la barre de navigation contient "Home | Privacy | Jeux | Ajouter un jeu | Mes Jeux".

---

## 13.4 Tester

1. Connecte-toi avec un compte A.
2. Crée 2 jeux.
3. Clique sur "Mes Jeux" → les 2 jeux s'affichent.
4. Déconnecte-toi, connecte-toi avec un compte B.
5. Clique sur "Mes Jeux" → page vide avec le bouton "Ajouter un jeu".
6. Crée 1 jeu avec B → il apparaît sur sa page "Mes Jeux", mais pas sur celle de A.

---

## ✅ Point de contrôle

- [ ] `/Game/MyGames` est accessible uniquement connecté.
- [ ] La page n'affiche que les jeux dont `UserId` == user connecté.
- [ ] Une liste vide affiche un message sympa + un bouton "Ajouter un jeu".
- [ ] Le lien "Mes Jeux" est dans la navbar uniquement quand connecté.

Direction → [Chapitre 14 : CRUD utilisateur](./14-crud-utilisateur.md).
