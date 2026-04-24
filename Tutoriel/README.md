# Tutoriel GamesApp – Correction pas à pas

Ce dossier contient la correction complète, étape par étape, de l'exercice **GamesApp** (ASP.NET Core MVC .NET 10, Entity Framework Core, Identity).

À la fin du tutoriel tu auras construit :

- une application MVC qui affiche une liste de jeux vidéo,
- un CRUD complet sur les jeux (Create / Read / Update / Delete),
- un système d'authentification avec inscription, connexion, déconnexion,
- une relation entre un utilisateur et ses jeux,
- une autorisation fine : seuls le **propriétaire** ou un **admin** peuvent modifier / supprimer un jeu,
- une page "Mes Jeux" qui n'affiche que les jeux de l'utilisateur connecté,
- un CRUD utilisateur (Details / Edit / Delete) avec gestion des rôles.

## Comment utiliser ce tutoriel

Tu es censé **lire les chapitres dans l'ordre** et coder en parallèle. Chaque chapitre :

1. explique **pourquoi** on fait la prochaine étape ;
2. donne **toutes** les commandes à taper et **tout** le code à copier ;
3. se termine par un "✅ Point de contrôle" pour vérifier que tout fonctionne avant de passer au suivant.

Si tu galères, compare ton code avec la correction finale dans le dossier `GamesApp/`.

## Sommaire

| # | Chapitre | Objectif |
|---|---|---|
| 01 | [Prérequis et création du projet](./01-prerequis-et-creation-du-projet.md) | Installer l'environnement, créer la solution |
| 02 | [Le modèle Game et la base de données](./02-modele-game-et-base-de-donnees.md) | Modèle, DbContext, configuration Fluent API, migration |
| 03 | [Le Repository Pattern](./03-repository-pattern.md) | Interface + implémentation + injection |
| 04 | [Afficher la liste et les détails d'un jeu](./04-afficher-les-jeux.md) | Controller, vues Index et Details, navbar |
| 05 | [Créer un jeu (Create)](./05-creer-un-jeu.md) | DTO + vue + POST |
| 06 | [Modifier un jeu (Edit)](./06-modifier-un-jeu.md) | DTO + vue + POST |
| 07 | [Supprimer un jeu (Delete)](./07-supprimer-un-jeu.md) | Vue de confirmation + POST |
| 08 | [Installer Identity et créer le User](./08-installer-identity.md) | Packages, User hérite d'IdentityUser, migration |
| 09 | [Inscription / Connexion / Déconnexion](./09-auth-controller.md) | AuthController + vues Login et Register |
| 10 | [Protéger les actions avec [Authorize]](./10-proteger-les-actions.md) | Cookies, navbar connectée, AccessDenied |
| 11 | [Relier un jeu à son utilisateur](./11-relation-user-game.md) | FK UserId, migration add-game-to-user |
| 12 | [Autoriser seulement le propriétaire ou l'admin](./12-autorisation-proprietaire-ou-admin.md) | IsInRole, contrôle serveur |
| 13 | [Page "Mes Jeux"](./13-mes-jeux.md) | Action MyGames + vue dédiée |
| 14 | [CRUD utilisateur](./14-crud-utilisateur.md) | UserController Details/Edit/Delete + vues |

## Conventions utilisées

- **VS** = Visual Studio 2022 (ou supérieur).
- **PMC** = Package Manager Console (`Outils → Gestionnaire de packages NuGet → Console`).
- Les commandes EF (`Add-Migration`, `Update-Database`) se font **dans la PMC**, pas en CMD.
- Le style de code est volontairement **simple et expressif** : constructeurs primaires, `async/await` partout, pas de service layer intermédiaire — le controller parle directement au repository.

Bonne route ! 🚀
