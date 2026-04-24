# Chapitre 14 – CRUD utilisateur

## 🎯 Objectif du chapitre (chapitre final)

Construire un `UserController` qui permet :

- d'**afficher** le profil d'un utilisateur (ses infos + ses jeux),
- de le **modifier** (propriétaire ou admin — l'admin peut en plus changer le rôle),
- de le **supprimer** (propriétaire ou admin).

---

## 14.1 Le DTO UpdateUserDTOs

Dans `Models/UserModels/`, nouvelle classe **UpdateUserDTOs.cs** :

```csharp
using System.ComponentModel.DataAnnotations;

namespace GamesApp.Models.UserModels
{
    public class UpdateUserDTOs
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
```

Pas de champ mot de passe ici : changer son mot de passe passe par `UserManager.ChangePasswordAsync` et c'est un sujet à part (à faire en TP avancé).

---

## 14.2 Le UserController

Clic droit sur `Controllers/` → `Ajouter → Contrôleur MVC - Vide` → **UserController**.

```csharp
using GamesApp.Data;
using GamesApp.Models.UserModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GamesApp.Controllers
{
    [Authorize]
    public class UserController(
        UserManager<User> _userManager,
        GameContext _context,
        RoleManager<IdentityRole> _roleManager) : Controller
    {
        // GET: UserController/Details/abc-123
        public async Task<ActionResult> Details(string id)
        {
            // On récupère l'utilisateur avec ses jeux (Include)
            User? user = await _context.Users
                .Include(u => u.MyGames)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return RedirectToAction("Index", "Game");
            }

            return View(user);
        }


        // GET: UserController/Edit/abc-123
        public async Task<ActionResult> Edit(string id)
        {
            User? user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return RedirectToAction("Index", "Game");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user.Id != currentUserId && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            UpdateUserDTOs userToUpdate = new()
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Nom = user.Nom,
                Prenom = user.Prenom,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty
            };

            await LoadRolesAsync();
            return View(userToUpdate);
        }

        // POST: UserController/Edit/abc-123
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UpdateUserDTOs updatedUser)
        {
            if (!ModelState.IsValid)
            {
                await LoadRolesAsync();
                return View(updatedUser);
            }

            try
            {
                User? user = await _userManager.FindByIdAsync(updatedUser.Id);

                if (user == null)
                {
                    return RedirectToAction("Index", "Game");
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (user.Id != currentUserId && !User.IsInRole("Admin"))
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }

                user.Email = updatedUser.Email;
                user.UserName = updatedUser.Email;
                user.Nom = updatedUser.Nom;
                user.Prenom = updatedUser.Prenom;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var err in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, err.Description);
                    }
                    await LoadRolesAsync();
                    return View(updatedUser);
                }

                // Gestion du rôle (admin uniquement)
                if (User.IsInRole("Admin"))
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var newRole = updatedUser.Role ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(newRole) && currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    }
                    else if (!string.IsNullOrWhiteSpace(newRole) && !currentRoles.Contains(newRole))
                    {
                        if (currentRoles.Any())
                        {
                            await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        }

                        if (!await _roleManager.RoleExistsAsync(newRole))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(newRole));
                        }

                        await _userManager.AddToRoleAsync(user, newRole);
                    }
                }

                return RedirectToAction("Details", new { id = user.Id });
            }
            catch
            {
                await LoadRolesAsync();
                return View(updatedUser);
            }
        }


        // GET: UserController/Delete/abc-123
        public async Task<ActionResult> Delete(string id)
        {
            User? user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return RedirectToAction("Index", "Game");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user.Id != currentUserId && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            return View(user);
        }

        // POST: UserController/Delete/abc-123
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            try
            {
                User? user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return RedirectToAction("Index", "Game");
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (user.Id != currentUserId && !User.IsInRole("Admin"))
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    return View(user);
                }

                return RedirectToAction("Index", "Game");
            }
            catch
            {
                return RedirectToAction("Index", "Game");
            }
        }

        // Charge les rôles dans ViewData pour le <select> de la vue Edit
        private async Task LoadRolesAsync()
        {
            var allRoles = await _roleManager.Roles
                .Select(r => r.Name!)
                .ToListAsync();
            ViewData["AllRoles"] = allRoles;
        }
    }
}
```

### Points pédagogiques importants

- **`[Authorize]` au niveau classe** : tous les endpoints sont protégés par défaut.
- **On utilise `_context.Users.Include(u => u.MyGames)`** dans `Details` parce que `UserManager.FindByIdAsync` ne peuple pas les navigation properties. Il faut passer par EF pour faire un `Include`.
- **Pattern `DeleteConfirmed`** : l'attribut `[ActionName("Delete")]` permet d'avoir deux méthodes dans le controller qui répondent à la même route (`GET /User/Delete/123` et `POST /User/Delete/123`), avec une signature différente, sans collision.
- **Gestion du rôle en 3 cas** : (1) admin met vide → retire les rôles, (2) admin change le rôle → retire + ajoute, (3) utilisateur non-admin → la modif du rôle est ignorée car le bloc `if (User.IsInRole("Admin"))` saute.
- **`LoadRolesAsync` est appelée sur TOUS les `return View(...)`** de la POST, sinon si la validation échoue la vue re-rend sans la liste des rôles et plante (`NullReferenceException`).

---

## 14.3 Les 3 vues

Dans `Views/`, nouveau dossier **User**.

### 14.3.1 Details.cshtml

```cshtml
@using System.Security.Claims
@model GamesApp.Models.UserModels.User

<h2 class="text-center mt-5 mb-5">Profil de @Model.Prenom @Model.Nom</h2>

<div class="card w-50 mx-auto mt-4 p-3">
    <div class="d-flex">
        <div>
            <p class="fw-bold ms-3 me-5">Prénom :</p>
            <p class="fw-bold ms-3 me-5">Nom :</p>
            <p class="fw-bold ms-3 me-5">Email :</p>
            <p class="fw-bold ms-3 me-5">Inscrit le :</p>
            <p class="fw-bold ms-3 me-5">Nb de jeux :</p>
        </div>
        <div>
            <p>@Model.Prenom</p>
            <p>@Model.Nom</p>
            <p>@Model.Email</p>
            <p>@Model.CreatedAt.ToShortDateString()</p>
            <p>@Model.MyGames.Count</p>
        </div>
    </div>

    @{
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool canEdit = Model.Id == currentUserId || User.IsInRole("Admin");
    }

    @if (canEdit)
    {
        <div class="d-flex justify-content-evenly mb-3">
            <a asp-controller="User" asp-action="Edit" asp-route-id="@Model.Id" class="btn btn-outline-secondary w-25">Edit</a>
            <a asp-controller="User" asp-action="Delete" asp-route-id="@Model.Id" class="btn btn-outline-danger w-25">Delete</a>
        </div>
    }
</div>

@if (Model.MyGames.Any())
{
    <h3 class="text-center mt-5 mb-3">Ses jeux</h3>
    <div>
        @foreach (GamesApp.Models.GameModels.Game g in Model.MyGames)
        {
            <div class="card w-75 mx-auto rounded shadow p-3 text-center m-2">
                <h5>@g.Name</h5>
                <p>@g.Description</p>
                <a asp-controller="Game" asp-action="Details" asp-route-id="@g.Id" class="btn btn-outline-primary w-25 mx-auto">Details</a>
            </div>
        }
    </div>
}
```

### 14.3.2 Edit.cshtml

```cshtml
@model GamesApp.Models.UserModels.UpdateUserDTOs

<h2 class="text-center mt-5 mb-5">Modifier mon profil</h2>

<div asp-validation-summary="ModelOnly" class="text-danger"></div>

<form class="card w-75 mx-auto text-center" method="post" asp-action="Edit">
    <input type="hidden" asp-for="Id" />

    <div class="p-2 m-3">
        <label asp-for="Nom" class="form-label"></label>
        <input asp-for="Nom" type="text" class="form-control w-50 mx-auto" />
        <span asp-validation-for="Nom" class="text-danger"></span>
    </div>

    <div class="p-2 m-3">
        <label asp-for="Prenom" class="form-label"></label>
        <input asp-for="Prenom" type="text" class="form-control w-50 mx-auto" />
        <span asp-validation-for="Prenom" class="text-danger"></span>
    </div>

    <div class="p-2 m-3">
        <label asp-for="Email" class="form-label"></label>
        <input asp-for="Email" type="email" class="form-control w-50 mx-auto" />
        <span asp-validation-for="Email" class="text-danger"></span>
    </div>

    @if (User.IsInRole("Admin"))
    {
        var roles = ViewData["AllRoles"] as IEnumerable<string> ?? new List<string>();
        <div class="p-2 m-3">
            <label asp-for="Role" class="form-label"></label>
            <select asp-for="Role" class="form-control w-50 mx-auto" asp-items="new Microsoft.AspNetCore.Mvc.Rendering.SelectList(roles)">
                <option value="">-- Aucun --</option>
            </select>
            <span asp-validation-for="Role" class="text-danger"></span>
        </div>
    }

    <div class="d-flex justify-content-evenly mb-5">
        <input type="submit" value="Update" class="btn btn-outline-success w-25" />
        <a asp-controller="User" asp-action="Details" asp-route-id="@Model.Id" class="btn btn-outline-secondary w-25">Annuler</a>
    </div>
</form>
```

Le `<select>` pour le rôle n'est affiché qu'à l'admin. Le fallback `?? new List<string>()` évite le crash si `ViewData["AllRoles"]` est null.

### 14.3.3 Delete.cshtml

```cshtml
@model GamesApp.Models.UserModels.User

<h2 class="text-center mt-5 mb-5">Supprimer @Model.Prenom @Model.Nom ?</h2>

<div class="card w-50 mx-auto p-3">
    <h4 class="text-center">Confirmation de suppression</h4>
    <hr />
    <dl class="row">
        <dt class="col-sm-3">Prénom</dt>
        <dd class="col-sm-9">@Model.Prenom</dd>

        <dt class="col-sm-3">Nom</dt>
        <dd class="col-sm-9">@Model.Nom</dd>

        <dt class="col-sm-3">Email</dt>
        <dd class="col-sm-9">@Model.Email</dd>

        <dt class="col-sm-3">Inscrit le</dt>
        <dd class="col-sm-9">@Model.CreatedAt.ToShortDateString()</dd>
    </dl>

    <form asp-action="Delete" method="post">
        <input type="hidden" name="id" value="@Model.Id" />
        <div class="d-flex justify-content-evenly mb-3">
            <input type="submit" value="Delete" class="btn btn-outline-danger w-25" />
            <a asp-controller="User" asp-action="Details" asp-route-id="@Model.Id" class="btn btn-outline-secondary w-25">Annuler</a>
        </div>
    </form>
</div>
```

---

## 14.4 Lien "profil" dans la navbar

Dans `Views/Shared/_Layout.cshtml`, transforme le `<span>Bonjour, ...</span>` en `<a>` cliquable :

```cshtml
<li class="nav-item">
    <a class="nav-link text-dark" asp-controller="User" asp-action="Details" asp-route-id="@UserManager.GetUserId(User)">
        👤 Bonjour, @User.Identity?.Name
    </a>
</li>
```

`UserManager.GetUserId(User)` récupère l'id du user courant plus proprement que `FindFirstValue(ClaimTypes.NameIdentifier)`.


---

## ✅ Point de contrôle final

- [ ] `/User/Details/{id}` affiche le profil + les jeux.
- [ ] `/User/Edit/{id}` fonctionne pour le propriétaire et pour l'admin, interdit aux autres.
- [ ] `/User/Delete/{id}` fonctionne pour le propriétaire et pour l'admin, interdit aux autres.
- [ ] L'admin voit un `<select>` de rôles dans Edit, les autres non.
- [ ] Changer le rôle via le select, puis recharger la page Edit, reflète bien le nouveau rôle.
- [ ] Mettre le rôle à "-- Aucun --" retire bien le rôle.
- [ ] Le lien "👤 Bonjour, X" dans la navbar pointe vers le profil.

---

# Role Admin

- Pour ajouter et affecter le role admin executer cette requette dans ssms : 

````sql
-- Créer le rôle
INSERT INTO AspNetRoles (Id, Name, NormalizedName)
VALUES (NEWID(), 'Admin', 'ADMIN');

INSERT INTO AspNetRoles (Id, Name, NormalizedName)
VALUES (NEWID(), 'User', 'USER');

-- L'assigner (remplace les IDs)
-- remplacer l'email !
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u, AspNetRoles r
WHERE u.Email = 'admin@mail.com'
```