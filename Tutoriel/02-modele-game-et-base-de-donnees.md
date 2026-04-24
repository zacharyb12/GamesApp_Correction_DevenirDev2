# Chapitre 2 – Le modèle Game et la base de données

## 🎯 Objectif du chapitre

Créer la classe `Game`, le `DbContext`, configurer la base avec la Fluent API, et générer la première migration qui crée la table `Games` en base.

---

## 2.1 Créer le dossier Models et le modèle Game

Clic droit sur le projet `GamesApp` → `Ajouter → Nouveau dossier` → nommer **Models**.
Dans `Models/`, clic droit → `Ajouter → Nouveau dossier` → **GameModels**.

Dans `Models/GameModels/`, clic droit → `Ajouter → Classe...` → nom **Game.cs**.

Remplace tout le contenu par :

```csharp
namespace GamesApp.Models.GameModels
{
    public class Game
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Editor { get; set; }

        public DateTime CreatedAt { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }
    }
}
```

> ℹ️ On ajoutera les propriétés liées à `User` (`UserId`, `User`) plus tard, au chapitre 11. Ne les mets pas maintenant, sinon la migration va casser.

---

## 2.2 Créer le DbContext

Clic droit sur le projet → nouveau dossier **Data**.
Dans `Data/`, clic droit → `Ajouter → Classe...` → **GameContext.cs**.

```csharp
using GamesApp.Models.GameModels;
using Microsoft.EntityFrameworkCore;

namespace GamesApp.Data
{
    public class GameContext : DbContext
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

> Plus tard (au chapitre 8), on changera l'héritage en `IdentityDbContext<User>`. Pour l'instant, on reste sur `DbContext`.

---

## 2.3 Configurer la table avec la Fluent API

Plutôt que de mettre des `[Required]` partout sur la classe `Game`, on va utiliser une **classe de configuration** dédiée. Plus propre et plus lisible.

Dans `Data/`, crée un sous-dossier **Configurations**, puis une classe **GameConfiguration.cs** :

```csharp
using GamesApp.Models.GameModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GamesApp.Data.Configurations
{
    public class GameConfiguration : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder.ToTable("Games");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Id)
                .ValueGeneratedOnAdd();

            builder.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(g => g.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(g => g.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(g => g.Quantity)
                .HasDefaultValue(0);

            builder.Property(g => g.ImageUrl)
                .IsRequired();

            builder.Property(g => g.Price)
                .HasColumnType("decimal(8,2)");

            builder.Property(g => g.Editor)
                .IsRequired()
                .HasMaxLength(50);
        }
    }
}
```

Ce que ce fichier fait :

- `ToTable("Games")` → la table s'appellera `Games` (et pas `Game`).
- `HasKey` + `ValueGeneratedOnAdd` → `Id` est clé primaire auto-incrémentée.
- `IsRequired()` + `HasMaxLength(...)` → contraintes NOT NULL + taille.
- `HasDefaultValueSql("GETDATE()")` → la date de création sera remplie par SQL.
- `HasDefaultValue(0)` → stock par défaut = 0.
- `HasColumnType("decimal(8,2)")` → 8 chiffres, 2 après la virgule.

C'est la méthode `ApplyConfigurationsFromAssembly` dans `GameContext.OnModelCreating` qui va trouver toute seule cette classe et l'appliquer.

---

## 2.4 La connection string

Ouvre `appsettings.json` et ajoute la section `ConnectionStrings` :

```json
{
  "ConnectionStrings": {
    "myConnection": "Server=(localdb)\\MSSQLLocalDB;Database=GamesAppDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> ⚠️ Attention au double backslash `\\` dans le JSON. Si tu n'utilises pas LocalDB, remplace par ton serveur (par ex. `Server=localhost;` ou `Server=localhost,1433;User Id=sa;Password=...;TrustServerCertificate=true`).

---

## 2.5 Enregistrer le DbContext dans Program.cs

Ouvre `Program.cs` et ajoute **après** `builder.Services.AddControllersWithViews();` :

```csharp
using GamesApp.Data;
using Microsoft.EntityFrameworkCore;

// ...
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<GameContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("myConnection"))
);
```

`AddDbContext` fait 2 choses :

1. il enregistre `GameContext` dans le conteneur d'injection de dépendances (on pourra donc l'injecter dans un repository) ;
2. il le configure pour utiliser SQL Server avec la chaîne de connexion `myConnection`.

---

## 2.6 La première migration

On va maintenant demander à EF de générer un script SQL qui crée la table `Games`.

Ouvre la **Package Manager Console** (`Outils → Gestionnaire de packages NuGet → Console`). Vérifie que *Projet par défaut* = `GamesApp`.

```powershell
Add-Migration init
```

EF doit créer un dossier `Migrations/` avec un fichier du style `20260420xxxx_init.cs`. Ouvre-le : tu dois voir `migrationBuilder.CreateTable("Games", ...)` avec toutes tes colonnes.

> Si l'erreur `Your target project 'GamesApp' doesn't match your migrations assembly` apparaît, vérifie que la PMC pointe bien sur `GamesApp` comme *projet par défaut*.

Puis applique la migration à la base :

```powershell
Update-Database
```

Résultat attendu : `Done.` et une base `GamesAppDb` qui contient la table `Games` + `__EFMigrationsHistory`.

### Vérifier dans SQL Server

Dans VS : `Affichage → Explorateur d'objets SQL Server` → déplie `(localdb)\MSSQLLocalDB` → `Bases de données` → `GamesAppDb` → `Tables`. Tu dois voir `dbo.Games` avec les bonnes colonnes.

---

## ✅ Point de contrôle

- [ ] La classe `Game` compile sans erreur.
- [ ] La classe `GameConfiguration` existe dans `Data/Configurations`.
- [ ] `Program.cs` enregistre bien `GameContext` avec `UseSqlServer`.
- [ ] La migration `init` existe dans `Migrations/`.
- [ ] La base `GamesAppDb` contient la table `dbo.Games`.

Direction → [Chapitre 3 : Le Repository Pattern](./03-repository-pattern.md).
