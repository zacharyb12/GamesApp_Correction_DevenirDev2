using GamesApp_Correction_DevenirDev2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace GamesApp_Correction_DevenirDev2.Controllers
{
    public class AuthController(UserManager<User> _userManager,SignInManager<User> _signInManager) : Controller
    {

        public IActionResult Register()
        {
            return View(new RegisterRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest form)
        {
           if(!ModelState.IsValid)
            {
                return View(form);
            }

            User u = new()
            {
                UserName = form.Email,
                Email = form.Email,
                Prenom = form.Prenom,
                Nom = form.Nom,
                CreatedAt = DateTime.Now
            };

            // ici on crée l'utilisateur en db
            var result = await _userManager.CreateAsync(u,form.Password);

            if(result.Succeeded)
            {// ici on défini que l'utilisateur est connecté
                await _signInManager.SignInAsync( u , isPersistent:false );
                return RedirectToAction("Index", "Game");
            }

            return View(form);
        }


        public IActionResult Login()
        {
            return View(new LoginRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest form)
        {
            if(!ModelState.IsValid)
            {
                return View(form);
            }

            var result = await _signInManager.PasswordSignInAsync(
                form.Email,
                form.Password,
                isPersistent: form.RememberMe,
                lockoutOnFailure : true
                );


            if(result.Succeeded)
            {
                return RedirectToAction("Index","Game");
            }

            return View(form);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Game");
        }

    }
}
