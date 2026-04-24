# Chapitre 8 – Installer Identity et créer le User

## 🎯 Objectif du chapitre

Brancher **ASP.NET Core Identity** sur notre projet. À la fin on aura :

- un modèle `User` qui hérite de `IdentityUser` (avec des champs custom : `Prenom`, `Nom`, `CreatedAt`),
- un `GameContext` qui gère à la fois les `Games` ET les tables Identity (`AspNetUsers`, `AspNetRoles`, etc.),
- une migration qui crée toute la plomberie Identity en base.

---

## 8.1 Créer le modèle User

Dans `Models/`, nouveau dossier **UserModels**.
Dans `Models/UserModels/`, nouvelle classe **User.cs** :

```csharp
using Microsoft.AspNetCore.Identity;

namespace GamesApp.Models.UserModels
{
    public class User : IdentityUser
    {
        public string Prenom { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

### Ce que ça nous apporte

`IdentityUser` fournit déjà :

- `Id` (string, GUID),
- `UserName`, `Email`, `EmailConfirmed`,
- `PasswordHash`,
- `LockoutEnd`, `AccessFailedCount`,
- `PhoneNumber`, `TwoFactorEnabled`,
- etc.

Nous, on ajoute juste `Prenom`, `Nom`, `CreatedAt`.

> ℹ️ On ajoutera la collection `MyGames` dans cette classe au chapitre 11, quand on fera la relation.

---

## 8.2 Faire hériter le GameContext d'IdentityDbContext

Ouvre `Data/GameContext.cs` et modifie l'héritage :

```csharp
using GamesApp.Models.GameModels;
using GamesApp.Models.UserModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GamesApp.Data
{
    public class GameContext : IdentityDbContext<User>
    {
        public GameContext(DbContextOptions<GameContext> options) : base(options)
        { }

        public DbSet<Game> Games { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(GameContext).Assembly);
        }
    }
}
```

Changements :

- `: DbContext` devient `: IdentityDbContext<User>`.
- On passe notre classe `User` en paramètre générique, pour qu'Identity sache quelle classe utiliser.
- `base.OnModelCreating(builder)` est **obligatoire** : c'est lui qui configure les tables Identity. Ne le supprime pas.

---

## 8.3 Configurer Identity dans Program.cs

Ajoute dans `Program.cs`, **après** le `AddScoped<IGameRepository, GameRepository>()` :

```csharp
using GamesApp.Models.UserModels;
using Microsoft.AspNetCore.Identity;

// ...

// Configuration de l'Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Règles de mot de passe
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;

    // Verrouillage après échecs
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    // Utilisateur
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // true en prod
})
.AddEntityFrameworkStores<GameContext>()
.AddDefaultTokenProviders();

// Configuration du cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Home/Index";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});
```

Ce qu'on vient de configurer :

- **Password policy** : 8 caractères, au moins un chiffre et une minuscule.
- **Lockout** : après 5 mauvais mots de passe, compte bloqué 15 minutes (anti brute-force).
- **Unique email** : impossible d'avoir deux comptes avec le même email.
- **RequireConfirmedEmail = false** : on ne force pas la confirmation par email (à activer en prod).
- **LoginPath / LogoutPath / AccessDeniedPath** : où rediriger l'utilisateur.
- **Sliding expiration** : le cookie se renouvelle à chaque requête (l'utilisateur n'est pas déconnecté tant qu'il reste actif).

---

## 8.4 Activer l'authentification dans le pipeline

Dans `Program.cs`, vérifie qu'`app.UseAuthorization();` est bien présent. Certains templates n'incluent que `UseAuthorization` ; avec Identity, `UseAuthentication` est ajouté automatiquement par la pipeline moderne. Si tu vois un comportement bizarre (`User.Identity.IsAuthenticated` toujours false), ajoute explicitement :

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Dans cet ordre et **entre** `UseRouting` et `MapControllerRoute`.

---

## 8.5 Générer la migration Identity

Ouvre la PMC :

```powershell
Add-Migration auth
```

Tu vas voir un gros fichier de migration qui crée :

- `AspNetUsers` (avec tes colonnes custom `Prenom`, `Nom`, `CreatedAt`),
- `AspNetRoles`,
- `AspNetUserRoles`,
- `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`.

Applique :

```powershell
Update-Database
```

Dans ton Explorateur d'objets SQL Server, rafraîchis la BDD → tu dois voir les nouvelles tables.

---

## 8.6 Mettre à jour _ViewImports.cshtml

Pour que les vues aient accès aux namespaces Identity, ouvre `Views/_ViewImports.cshtml` et ajoute :

```cshtml
@using GamesApp
@using GamesApp.Models
@using GamesApp.Models.UserModels
@using Microsoft.AspNetCore.Identity
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

> 💡 Ça évite de devoir `@using` dans chaque vue.

---

## ✅ Point de contrôle

- [ ] La classe `User` hérite de `IdentityUser` et compile.
- [ ] `GameContext` hérite de `IdentityDbContext<User>`.
- [ ] `Program.cs` contient `AddIdentity<User, IdentityRole>` et `ConfigureApplicationCookie`.
- [ ] La migration `auth` a été appliquée → les tables `AspNetUsers`, `AspNetRoles`, etc. existent en base.
- [ ] Le projet compile et démarre toujours (même si on n'a pas encore de pages de login).

Direction → [Chapitre 9 : Inscription / Connexion / Déconnexion](./09-auth-controller.md).
