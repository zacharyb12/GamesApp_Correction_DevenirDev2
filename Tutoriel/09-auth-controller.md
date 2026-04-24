# Chapitre 9 – Inscription / Connexion / Déconnexion

## 🎯 Objectif du chapitre

Construire un `AuthController` qui expose `Register`, `Login` et `Logout`, avec leurs vues Razor, en s'appuyant sur les services `UserManager<User>` et `SignInManager<User>` fournis par Identity.

---

## 9.1 Les DTO de formulaire

Dans `Models/`, nouveau dossier **AuthModels**.

### RegisterRequest

Dans `Models/AuthModels/`, nouvelle classe **RegisterRequest.cs** :

```csharp
using System.ComponentModel.DataAnnotations;

namespace GamesApp.Models.AuthModels
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Le prénom est requis")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est requis")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères, une majuscule, une minuscule et un chiffre")]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmation est requise")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
        [Display(Name = "Confirmer le mot de passe")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
```

Les DataAnnotations utilisées :

- `[Required]` : champ obligatoire.
- `[EmailAddress]` : valide le format email.
- `[StringLength(max, MinimumLength = x)]` : longueur min / max.
- `[Compare("Autre champ")]` : les deux doivent être égaux (utile pour la confirmation).
- `[DataType(DataType.Password)]` : transforme l'input en `type="password"`.
- `[Display(Name = "...")]` : libellé affiché par `<label asp-for>`.

### LoginRequest

Dans le même dossier, **LoginRequest.cs** :

```csharp
using System.ComponentModel.DataAnnotations;

namespace GamesApp.Models.AuthModels
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Se souvenir de moi")]
        public bool RememberMe { get; set; }
    }
}
```

---

## 9.2 Créer le AuthController

Clic droit sur `Controllers/` → `Ajouter → Contrôleur MVC - Vide` → **AuthController**.

```csharp
using GamesApp.Models.AuthModels;
using GamesApp.Models.UserModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GamesApp.Controllers
{
    // SERVICES IDENTITY :
    // UserManager : Gestion des comptes utilisateurs (création, modification, suppression)
    // SignInManager : Gestion de l'authentification (connexion, déconnexion, cookies)
    public class AuthController(UserManager<User> _userManager, SignInManager<User> _signInManager) : Controller
    {
        // ================== INSCRIPTION ==================

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Prenom = model.Prenom,
                Nom = model.Nom,
                CreatedAt = DateTime.UtcNow
            };

            // CreateAsync : Crée le compte avec hachage automatique du mot de passe
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Connexion automatique après inscription
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Game");
            }

            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }
            return View(model);
        }

        // ================== CONNEXION ==================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Game");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Compte verrouillé. Réessayez plus tard.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
            return View(model);
        }

        // ================== DÉCONNEXION ==================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Game");
        }

        // ================== ACCÈS REFUSÉ ==================

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}
```

### Les points importants

- **`UserManager<User>.CreateAsync(user, password)`** : stocke le user en BDD et **hache** le mot de passe (PBKDF2 par défaut). Jamais de mot de passe en clair.
- **`SignInManager<User>.PasswordSignInAsync(...)`** : vérifie identifiants + génère le cookie d'auth.
- **`lockoutOnFailure: true`** : chaque mauvais mdp incrémente `AccessFailedCount` → bloque après 5 essais (configuré au chap. 8).
- **`Logout` en POST** : jamais en GET (sinon un lien malveillant pourrait te déconnecter).
- **`AccessDenied`** : page affichée quand un utilisateur connecté essaie d'accéder à une ressource interdite (rôle insuffisant).

---

## 9.3 Créer les vues Register et Login

### 9.3.1 Dossier Auth

Dans `Views/`, nouveau dossier **Auth**.

### 9.3.2 Register.cshtml

```cshtml
@model GamesApp.Models.AuthModels.RegisterRequest
<h2>Inscription</h2>

<div asp-validation-summary="ModelOnly" class="text-danger"></div>

<form asp-action="Register" method="post">
    <div class="mb-3">
        <label asp-for="Prenom"></label>
        <input asp-for="Prenom" class="form-control" />
        <span asp-validation-for="Prenom" class="text-danger"></span>
    </div>
    <div class="mb-3">
        <label asp-for="Nom"></label>
        <input asp-for="Nom" class="form-control" />
        <span asp-validation-for="Nom" class="text-danger"></span>
    </div>
    <div class="mb-3">
        <label asp-for="Email"></label>
        <input asp-for="Email" class="form-control" />
        <span asp-validation-for="Email" class="text-danger"></span>
    </div>
    <div class="mb-3">
        <label asp-for="Password"></label>
        <input asp-for="Password" class="form-control" />
        <span asp-validation-for="Password" class="text-danger"></span>
    </div>
    <div class="mb-3">
        <label asp-for="ConfirmPassword"></label>
        <input asp-for="ConfirmPassword" class="form-control" />
        <span asp-validation-for="ConfirmPassword" class="text-danger"></span>
    </div>
    <button type="submit" class="btn btn-primary">S'inscrire</button>
</form>
```

### 9.3.3 Login.cshtml

```cshtml
@model GamesApp.Models.AuthModels.LoginRequest

<h2>Connexion</h2>

<div asp-validation-summary="ModelOnly" class="text-danger"></div>

<form asp-action="Login" method="post">
    <input type="hidden" name="returnUrl" value="@ViewData["ReturnUrl"]" />
    <div class="mb-3">
        <label asp-for="Email"></label>
        <input asp-for="Email" class="form-control" />
        <span asp-validation-for="Email" class="text-danger"></span>
    </div>
    <div class="mb-3">
        <label asp-for="Password"></label>
        <input asp-for="Password" class="form-control" />
        <span asp-validation-for="Password" class="text-danger"></span>
    </div>
    <div class="mb-3 form-check">
        <input asp-for="RememberMe" class="form-check-input" />
        <label asp-for="RememberMe" class="form-check-label"></label>
    </div>
    <button type="submit" class="btn btn-primary">Se connecter</button>
</form>
```

### 9.3.4 AccessDenied.cshtml

```cshtml
<h2>Accès refusé</h2>
<p>Vous n'avez pas les droits pour accéder à cette page.</p>
<a asp-controller="Game" asp-action="Index" class="btn btn-outline-primary">Retour</a>
```

---

## 9.4 Tester l'inscription et la connexion

- Va sur `/Auth/Register` → inscris-toi avec un email + mdp valides.
- Tu es automatiquement connecté et redirigé vers `/Game`.
- Va sur `/Auth/Login` → reconnecte-toi avec le même compte.
- Tu peux vérifier dans `AspNetUsers` que ton utilisateur est bien enregistré (avec `PasswordHash` non null, `Prenom` et `Nom` remplis).

---

## ✅ Point de contrôle

- [ ] `/Auth/Register` affiche le formulaire et crée un utilisateur en BDD.
- [ ] `/Auth/Login` permet de se connecter.
- [ ] La table `AspNetUsers` contient le nouvel utilisateur avec `PasswordHash` rempli.
- [ ] Les erreurs de validation s'affichent correctement (essaie un email invalide, un mdp trop court).
- [ ] Un mauvais mot de passe affiche *"Email ou mot de passe incorrect"*.
- [ ] Après 5 mauvais mots de passe, le message devient *"Compte verrouillé"*.

Direction → [Chapitre 10 : Protéger les actions avec [Authorize]](./10-proteger-les-actions.md).
