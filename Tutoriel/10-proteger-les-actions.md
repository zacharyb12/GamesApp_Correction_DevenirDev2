# Chapitre 10 – Protéger les actions avec [Authorize]

## 🎯 Objectif du chapitre

Interdire aux utilisateurs **non connectés** de créer, modifier ou supprimer un jeu. On ajoute aussi une navbar dynamique : les liens "Connexion / Inscription" s'affichent quand on est déconnecté, et "Bonjour, X / Déconnexion" quand on est connecté.

---

## 10.1 Protéger le GameController

Ouvre `Controllers/GameController.cs`. Ajoute `[Authorize]` **au-dessus** des actions Create, Edit, Delete :

```csharp
using Microsoft.AspNetCore.Authorization;

// ...

// GET: GameController/Create
[Authorize]
public ActionResult Create()
{
    return View(new CreateGameDTOs());
}

// POST: GameController/Create
[HttpPost]
[Authorize]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Create(CreateGameDTOs newGame)
{
    // ... inchangé
}

// GET: GameController/Edit/5
[Authorize]
public async Task<ActionResult> Edit(int id)
{
    // ... inchangé
}

// POST: GameController/Edit/5
[HttpPost]
[Authorize]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Edit(UpdateGameDTOs updatedGame)
{
    // ... inchangé
}

// GET: GameController/Delete/5
[Authorize]
public async Task<ActionResult> Delete(int id)
{
    // ... inchangé
}

// POST: GameController/Delete/5
[Authorize]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Delete(Game game)
{
    // ... inchangé
}
```

### Comportement de [Authorize]

- Si l'utilisateur **n'est pas connecté** → redirige vers `/Auth/Login?returnUrl=/Game/Create` (grâce au `LoginPath` configuré au chap. 8).
- Si l'utilisateur **est connecté mais n'a pas le rôle requis** → redirige vers `AccessDenied`.
- On laisse `Index` et `Details` accessibles à tous (pas de `[Authorize]`).

> 💡 Alternative : mettre `[Authorize]` au niveau de la **classe** et `[AllowAnonymous]` sur `Index` et `Details`. Les deux approches sont valables. Moi je préfère l'explicite sur chaque action ici, c'est plus lisible pour des débutants.

---

## 10.2 Rendre la navbar dynamique

Ouvre `Views/Shared/_Layout.cshtml`. En haut du fichier, ajoute les injections :

```cshtml
@* INJECTION DE SERVICES dans la vue Razor *@
@inject SignInManager<User> SignInManager
@inject UserManager<User> UserManager
@using GamesApp.Models.UserModels
@using Microsoft.AspNetCore.Identity
```

`@inject` permet de récupérer un service directement dans une vue. Pratique pour vérifier l'état de connexion sans passer par un `ViewModel`.

### 10.2.1 Masquer "Ajouter un jeu" aux non-connectés

Toujours dans `_Layout.cshtml`, entoure le `<li>` "Ajouter un jeu" d'un `@if` :

```html
@if (SignInManager.IsSignedIn(User))
{
    <li class="nav-item">
        <a class="nav-link text-dark" asp-controller="Game" asp-action="Create">Ajouter un jeu</a>
    </li>
}
```

### 10.2.2 Afficher le menu utilisateur à droite

À la fin de la `<div class="navbar-collapse ...">`, ajoute un **second** `<ul class="navbar-nav">` (celui de droite) :

```html
@if (SignInManager.IsSignedIn(User))
{
    <ul class="navbar-nav">
        <li class="nav-item">
            <span class="nav-link">👤 Bonjour, @User.Identity?.Name</span>
        </li>
        <li class="nav-item">
            <form asp-action="Logout" asp-controller="Auth" method="post" class="d-inline">
                <button type="submit" class="nav-link btn btn-link text-dark">
                    🚪 Déconnexion
                </button>
            </form>
        </li>
    </ul>
}
else
{
    <ul class="navbar-nav">
        <li class="nav-item">
            <a class="nav-link" asp-action="Login" asp-controller="Auth">🔓 Connexion</a>
        </li>
        <li class="nav-item">
            <a class="nav-link" asp-action="Register" asp-controller="Auth">📝 Inscription</a>
        </li>
    </ul>
}
```

### Pourquoi Logout est un formulaire POST ?

Parce que **jamais d'action qui modifie l'état via GET**. Si Logout était un GET, un lien `<img src="/Auth/Logout">` dans un mail te déconnecterait sans même cliquer.

---

## 10.3 Tester

1. **Déconnecte-toi** (si tu es encore connecté).
2. Essaie d'aller sur `/Game/Create` → tu dois être redirigé sur `/Auth/Login?returnUrl=%2FGame%2FCreate`.
3. Connecte-toi → tu reviens sur `/Game/Create`.
4. Déconnecte-toi via le bouton de la navbar → retour sur `/Game`.

---

## ✅ Point de contrôle

- [ ] `[Authorize]` est bien présent sur Create / Edit / Delete (GET et POST).
- [ ] `/Game/Create` redirige vers `/Auth/Login` si déconnecté.
- [ ] La navbar affiche "Bonjour, {email}" + bouton Déconnexion quand connecté.
- [ ] La navbar affiche "Connexion / Inscription" quand déconnecté.
- [ ] Le bouton Déconnexion fonctionne et ramène à `/Game`.

Direction → [Chapitre 11 : Relier un jeu à son utilisateur](./11-relation-user-game.md).
