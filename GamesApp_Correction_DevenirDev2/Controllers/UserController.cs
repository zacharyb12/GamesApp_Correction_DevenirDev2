using GamesApp_Correction_DevenirDev2.Models;
using GamesApp_Correction_DevenirDev2.Repositories.UserRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GamesApp_Correction_DevenirDev2.Controllers
{
    public class UserController(IUserRepository _repository) : Controller
    {
        public async Task<IActionResult> Details()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                return RedirectToAction("Login","Auth");
            }

            User? u = await _repository.GetUserById(userId);
            if(u == null)
            {
                return RedirectToAction("Register", "Auth");
            }

            UserDetails userDetails = new()
            {
                Nom = u.Nom,
                Prenom = u.Prenom,
                Email = u.Email,
                CreatedAt = u.CreatedAt,
                GameCount = u.MyGames.Count
            };

            return View(userDetails);
        }


        public async Task<IActionResult> Edit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id)
        {
            return View();
        }

        public async Task<IActionResult> Delete()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed()
        {
            return View();
        }
    }
}
