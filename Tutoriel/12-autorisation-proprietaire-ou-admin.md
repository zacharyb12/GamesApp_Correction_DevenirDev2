# Chapitre 12 – Autoriser seulement le propriétaire ou l'admin

## 🎯 Objectif du chapitre

Actuellement `[Authorize]` suffit à n'importe quel utilisateur connecté pour modifier ou supprimer **n'importe quel** jeu. Inacceptable. On va :

1. bloquer `Edit` / `Delete` au **propriétaire** du jeu (celui dont `UserId` == user connecté) ;
2. laisser les utilisateurs ayant le rôle **Admin** faire ce qu'ils veulent ;
3. seeder le rôle Admin + un compte admin de test.

---

## 12.1 Comprendre `User.IsInRole` et `ClaimTypes.NameIdentifier`

Dans un controller ASP.NET Core :

- `User` est le `ClaimsPrincipal` du user connecté (fourni par Identity via le cookie).
- `User.IsInRole("Admin")` retourne `true` si l'utilisateur est dans ce rôle.
- `User.FindFirstValue(ClaimTypes.NameIdentifier)` retourne son `Id`.

Pour que `User.IsInRole("Admin")` fonctionne, il faut que **le rôle Admin existe** et **qu'au moins un user y soit affecté**. C'est ce qu'on va régler en fin de chapitre.

---

## 12.2 Sécuriser Edit dans GameController

Ouvre `GameController.cs`. Modifie la **GET** `Edit` :

```csharp
// GET: GameController/Edit/5
[Authorize]
public async Task<ActionResult> Edit(int id)
{
    Game? game = await _repository.GetById(id);

    if (game == null)
    {
        return RedirectToAction("Index");
    }

    // Seul le propriétaire du jeu ou un admin peut modifier
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (game.UserId != userId && !User.IsInRole("Admin"))
    {
        return RedirectToAction("AccessDenied", "Auth");
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
```

Et la **POST** `Edit` : on **recharge** le jeu en BDD pour vérifier le propriétaire côté serveur. Ne jamais faire confiance au formulaire.

```csharp
// POST: GameController/Edit/5
[HttpPost]
[Authorize]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Edit(UpdateGameDTOs updatedGame)
{
    try
    {
        // On recharge le jeu pour contrôler le propriétaire côté serveur
        Game? game = await _repository.GetById(updatedGame.Id);

        if (game == null)
        {
            return RedirectToAction("Index");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (game.UserId != userId && !User.IsInRole("Admin"))
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

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

> 🎓 **Point pédagogique important** : pourquoi recharger le jeu dans le POST ? Parce qu'un attaquant peut forger une requête POST avec n'importe quel `UpdateGameDTOs`. Même si l'UI ne montre pas le bouton Edit, rien n'empêche `curl` ou Postman de taper directement l'endpoint. La seule source de vérité du propriétaire est **la BDD**, pas le formulaire.

---

## 12.3 Sécuriser Delete dans GameController

Même logique, GET + POST :

```csharp
// GET: GameController/Delete/5
[Authorize]
public async Task<ActionResult> Delete(int id)
{
    Game? game = await _repository.GetById(id);

    if (game == null)
    {
        return RedirectToAction("Index");
    }

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (game.UserId != userId && !User.IsInRole("Admin"))
    {
        return RedirectToAction("AccessDenied", "Auth");
    }

    return View(game);
}

// POST: GameController/Delete/5
[Authorize]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Delete(Game game)
{
    try
    {
        Game? gameDb = await _repository.GetById(game.Id);

        if (gameDb == null)
        {
            return RedirectToAction("Index");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (gameDb.UserId != userId && !User.IsInRole("Admin"))
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

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

---

## 12.4 Masquer les boutons dans l'UI (cosmétique)

Le contrôle serveur est fait, mais c'est désagréable de voir "Edit / Delete" sur un jeu qu'on ne peut pas toucher. On masque dans `Views/Game/Details.cshtml` en remplaçant le bouton Edit par :

```cshtml
@using System.Security.Claims

@{
    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    bool canEdit = Model.UserId == currentUserId || User.IsInRole("Admin");
}

@if (canEdit)
{
    <a asp-controller="Game" asp-action="Edit" asp-route-id="@Model.Id" class="btn btn-outline-secondary mx-auto mb-3">Edit</a>
}
```

> 🚨 **À rappeler aux stagiaires** : **cacher le bouton n'est pas une sécurité**. La sécurité, c'est le `if` dans le controller (§ 12.2 et 12.3). Le masquage côté vue est juste pour l'UX.

---

## 12.5 Seeder le rôle Admin et un compte admin

Pour que `IsInRole("Admin")` retourne `true` pour quelqu'un, il faut que (1) le rôle existe et (2) l'utilisateur y soit affecté. On va faire ça au démarrage de l'application.

Dans `Program.cs`, **juste avant** `app.Run();`, ajoute :

```csharp
// Seed du rôle Admin et d'un compte admin
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    const string adminEmail = "admin@gamesapp.local";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            Prenom = "Super",
            Nom = "Admin",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(admin, "Admin1234");
    }

    if (!await userManager.IsInRoleAsync(admin, "Admin"))
    {
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}

app.Run();
```

> ⚠️ Il faudra rendre la méthode `Main` `async Task` (ou faire un `.GetAwaiter().GetResult()` partout). Plus simple :
>
> ```csharp
> public static async Task Main(string[] args)
> ```
>
> Change la signature de `Main` pour autoriser `await`.

Redémarre. Connecte-toi avec `admin@gamesapp.local` / `Admin1234` : tu peux maintenant éditer / supprimer n'importe quel jeu.

---

## 12.6 Tester

- Connecte-toi avec un compte **non-admin** X.
- Crée un jeu avec le compte X → tu peux l'éditer et le supprimer.
- Crée un second compte Y.
- Connecté avec Y, va sur `/Game/Edit/{id du jeu de X}` → tu dois être redirigé vers `AccessDenied`.
- Déconnecte-toi, reconnecte-toi avec l'admin → tu peux éditer / supprimer le jeu de X.

---

## ✅ Point de contrôle

- [ ] Un user non-propriétaire non-admin est redirigé vers `AccessDenied` sur `/Game/Edit/{id}` et `/Game/Delete/{id}`.
- [ ] Le propriétaire peut éditer / supprimer son propre jeu.
- [ ] Un admin peut éditer / supprimer n'importe quel jeu.
- [ ] Le bouton "Edit" dans Details apparaît seulement pour le propriétaire ou un admin.
- [ ] Le seed au démarrage crée le rôle `Admin` et le user `admin@gamesapp.local`.

Direction → [Chapitre 13 : Page "Mes Jeux"](./13-mes-jeux.md).
