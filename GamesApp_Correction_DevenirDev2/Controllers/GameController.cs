using GamesApp_Correction_DevenirDev2.Models;
using GamesApp_Correction_DevenirDev2.Repositories.GamesRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GamesApp_Correction_DevenirDev2.Controllers
{
    public class GameController(IGameRepository _repository) : Controller
    {
        // GET: GameController1
        public async Task<ActionResult> Index()
        {
            IEnumerable<Game> games = await _repository.GetGames();

            return View(games);
        }


        // GET: GameController1/Details/5
        public async Task<ActionResult> Details(int id)
        {
            Game? game = await _repository.GetGameById(id);

            if(game == null)
            {
                return RedirectToAction("Index");
            }

            return View(game);
        }


        // GET: GameController1/Create
        [Authorize]
        public ActionResult Create()
        {
            return View(new CreateGameDTOs());
        }

        // POST: GameController1/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateGameDTOs newGame)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return View(newGame);
                }

                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if(userId == null)
                {
                    return RedirectToAction("Index");
                }

                bool result = await _repository.CreateGame(newGame,userId);

                if(!result)
                {
                    return View(newGame);
                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View(newGame);
            }
        }



        // GET: GameController1/Edit/5
        [Authorize]
        public async Task<ActionResult> Edit(int id)
        {
            Game? game = await _repository.GetGameById(id);


            if(game == null)
            {
                return RedirectToAction("Index");
            }

            UpdateGameDTOs gameToUpdate = new()
            {
                Id = game.Id,
                Name = game.Name,
                Description = game.Description,
                Price = game.Price
            };

            return View(gameToUpdate);
        }

        // POST: GameController1/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UpdateGameDTOs updatedGame)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return View(updatedGame);
                }

                bool result = await _repository.UpdateGame(updatedGame);

                if(!result)
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


        // GET: GameController1/Delete/5
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            Game? game = await _repository.GetGameById(id);

            if(game == null)
            {
                return RedirectToAction("Index");
            }

            return View(game);
        }

        // POST: GameController1/Delete/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id,Game gameToDelete)
        {
            try
            {
                bool result = await _repository.DeleteGame(id);

                if(!result)
                {
                    return View(gameToDelete);
                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View(gameToDelete);
            }
        }

        public async Task<IActionResult> MyGames()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                return RedirectToAction("Index");
            }

            IEnumerable<Game> games = await _repository.GetMyGames(userId);

            return View(games);
        }
    }
}