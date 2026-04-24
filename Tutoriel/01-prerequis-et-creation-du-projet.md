# Chapitre 1 – Prérequis et création du projet

## 🎯 Objectif du chapitre

Installer l'environnement de travail, créer une nouvelle solution ASP.NET Core MVC avec .NET 10, et vérifier que le squelette généré démarre bien.

---

## 1.1 Ce qu'il faut installer

Avant toute chose, assure-vous d'avoir :

- **Visual Studio avec **ASP.NET et développement Web**.
- Le **SDK .NET 10** installé (vérifie avec `dotnet --version` dans une console → tu dois voir une version qui commence par `10.`).
- **SQL Server LocalDB** (inclus avec Visual Studio si tu as coché "Stockage de données et traitement"). Pour vérifier : `sqllocaldb info` dans une console → tu dois voir `MSSQLLocalDB`.
- Une extension SQL dans VS pour inspecter la base (Explorateur d'objets SQL Server ou SSMS).

---

## 1.2 Créer la solution

### Étape 1 – Nouveau projet

1. Ouvrir Visual Studio.
2. `Créer un projet` → filtrer par **C#** et **Web**.
3. Choisir le modèle **"ASP.NET Core Web App (Model-View-Controller)"** (PAS "Razor Pages", PAS "Web API").
4. Cliquer **Suivant**.

### Étape 2 – Configurer le projet

- **Nom du projet** : `GamesApp`
- **Nom de la solution** : `GamesApp`
- **Emplacement** : là où tu veux

Cliquer **Suivant**.

### Étape 3 – Informations supplémentaires

- **Framework** : `.NET 10.0`
- **Type d'authentification** : `Aucun` (on fera Identity nous-mêmes plus tard)
- **Configurer pour HTTPS** : ✅ coché
- **Activer Docker** : ❌ décoché
- **Ne pas utiliser d'instructions de haut niveau** : ❌ décoché

Cliquer **Créer**.

---

## 1.3 Premier démarrage

Appuie sur **F5** (ou le bouton ▶️ vert).

Une page d'accueil doit s'ouvrir dans ton navigateur avec l'URL `https://localhost:xxxx/` et un gros "Welcome".

> Si rien ne se passe ou qu'une erreur de certificat apparaît : fais `dotnet dev-certs https --trust` dans une console avec droits admin.

**Arrête l'application** (bouton 🟥 ou ferme l'onglet). On ne va pas rester sur cette page.

---

## 1.4 Ce qui vient d'être créé :

Ouvre l'Explorateur de solutions. Tu dois voir :

```
GamesApp/
├── Controllers/
│   └── HomeController.cs
├── Models/
│   └── ErrorViewModel.cs
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── GamesApp.csproj
```

Les concepts de base :

- **Controllers** : les classes qui reçoivent les requêtes HTTP.
- **Models** : les objets métier (on en ajoutera plein).
- **Views** : les fichiers `.cshtml` qui génèrent le HTML.
- **wwwroot** : les fichiers statiques servis tels quels (CSS, JS, images).
- **Program.cs** : le point d'entrée et la configuration des services.
- **appsettings.json** : la config (on y mettra la connection string).

---

## 1.5 Installer les packages NuGet

On va tout installer d'un coup pour les chapitres suivants. Ouvre `Outils → Gestionnaire de packages NuGet → Console du gestionnaire de package` (**PMC**). Vérifie que *"Projet par défaut"* est bien `GamesApp`, puis tape :

```powershell
Install-Package Microsoft.EntityFrameworkCore
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Microsoft.AspNetCore.Identity.EntityFrameworkCore
Install-Package Microsoft.AspNetCore.Identity.UI
Install-Package BCrypt.Net-Next
```

> ℹ️ `BCrypt.Net-Next` n'est utilisé que marginalement (dans un ancien repository). Identity hache déjà les mots de passe tout seul.

Vérifie dans `GamesApp.csproj` que tu as bien ces `PackageReference`. Si un n'est pas là, relance juste sa commande.

---

## ✅ Point de contrôle

Avant de passer au chapitre 2 :

- [ ] Le projet démarre avec F5 et affiche la page d'accueil par défaut.
- [ ] Les 6 packages NuGet apparaissent dans le `.csproj`.
- [ ] Aucune erreur en rouge dans la *Liste d'erreurs*.

Si tout est bon → [Chapitre 2 : Le modèle Game et la base de données](./02-modele-game-et-base-de-donnees.md).
