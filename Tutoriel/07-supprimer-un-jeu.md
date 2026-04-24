# Chapitre 7 – Supprimer un jeu (Delete)

## 🎯 Objectif du chapitre

Ajouter un écran de confirmation avant la suppression définitive d'un jeu. Classique ASP.NET MVC : une action GET qui affiche les infos + un formulaire qui POST pour confirmer.

> 🛡️ **Règle de sécurité** : on ne supprime **jamais** sur un GET. Un GET doit toujours être *safe* (sans effet de bord). Sinon un simple `<img src="/Game/Delete/1">` dans un email pourrait supprimer un jeu 😱.

---

## 7.1 Les 2 actions Delete dans le controller

Dans `GameController.cs`, ajoute :

```csharp
// GET: GameController/Delete/5
public async Task<ActionResult> Delete(int id)
{
    Game? game = await _repository.GetById(id);

    if (game == null)
    {
        return RedirectToAction("Index");
    }
    return View(game);
}

// POST: GameController/Delete/5
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Delete(Game game)
{
    try
    {
        bool response = await _repository.Delete(game.Id);

        if (!response)
        {
            return View(game);
        }

        return RedirectToAction("Index");
    }
    catch
    {
        return View(game);
    }
}
```

### Remarques

- La GET affiche le jeu dans la vue de confirmation.
- La POST reçoit directement un `Game` (parce que le formulaire envoie tous les champs via les `<input type="hidden">` ou un binding sur l'id).
- En pratique on n'a besoin **que** de l'`Id` pour supprimer — la vue ci-dessous exploite d'ailleurs uniquement cet id.

---

## 7.2 Créer la vue Delete

Dans `Views/Game/`, nouvelle vue **Delete.cshtml** :

```cshtml
@model GamesApp.Models.GameModels.Game

@{
    ViewData["Title"] = "Delete";
}

<h1>Delete</h1>

<h3>Are you sure you want to delete this?</h3>
<div>
    <h4>Game</h4>
    <hr />
    <dl class="row">
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Id)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Id)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Name)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Name)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Description)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Description)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Editor)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Editor)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.CreatedAt)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.CreatedAt)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Quantity)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Quantity)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Price)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Price)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.ImageUrl)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.ImageUrl)
        </dd>
    </dl>

    <form asp-action="Delete">
        <input type="submit" value="Delete" class="btn btn-danger" /> |
        <a asp-action="Index">Back to List</a>
    </form>
</div>
```

Ce qui est important :

- Les `@Html.DisplayFor` récupèrent automatiquement chaque propriété du model.
- Le `<form asp-action="Delete">` va POST sur `/Game/Delete/{id}` (l'id est dans l'URL courante).
- Pas besoin de répéter l'`Id` en champ caché puisque la route le transporte — mais en production je te recommande quand même un `<input type="hidden" asp-for="Id" />` par sécurité.

---

## 7.3 Tester

Lance, va sur `/Game/Delete/1` (remplace 1 par un id existant), la page de confirmation s'affiche. Clique "Delete" → retour sur `/Game`, le jeu a disparu.

---

## ✅ Point de contrôle

- [ ] `/Game/Delete/{id}` affiche la confirmation.
- [ ] Cliquer "Delete" supprime bien le jeu de la BDD.
- [ ] L'URL après suppression est `/Game` et le jeu n'apparaît plus.
- [ ] Le lien "Back to List" fonctionne sans supprimer.

**Fin de la partie CRUD "anonyme".** À partir du chapitre suivant on ajoute l'authentification.

Direction → [Chapitre 8 : Installer Identity et créer le User](./08-installer-identity.md).
