using GamesApp_Correction_DevenirDev2.Data;
using GamesApp_Correction_DevenirDev2.Models;
using Microsoft.EntityFrameworkCore;

namespace GamesApp_Correction_DevenirDev2.Repositories.GamesRepositories
{
    public class GameRepository(GameContext _context) : IGameRepository
    {
        public async Task<bool> CreateGame(CreateGameDTOs newGame,string userId)
        {

            Game game = new()
            {
                Name = newGame.Name,
                Description = newGame.Description,
                Price = newGame.Price,
                Editor = newGame.Editor,
                UserId = userId
            };

            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();

            if(game.Id == 0)
            {
                return false;
            }

            return true;
        }
        public async Task<IEnumerable<Game>> GetGames()
        {
            return await _context.Games.ToListAsync();
        }
        public async Task<Game?> GetGameById(int id)
        {
            Game? game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id);

            if(game == null)
            {
                return null;
            }

            return game;
        }
        public async Task<bool> DeleteGame(int id)
        {
            Game? game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id);

            if(game == null)
            {
                return false;
            }

            _context.Remove(game);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> UpdateGame(UpdateGameDTOs updatedGame)
        {
            Game? game = await _context.Games.FirstOrDefaultAsync(g => g.Id == updatedGame.Id);

            if (game == null)
            {
                return false;
            }

            game.Name = updatedGame.Name;
            game.Description = updatedGame.Description;
            game.Price = updatedGame.Price;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Game>> GetMyGames(string userId)
        {
            return await _context.Games.Where(g => g.UserId == userId).ToListAsync();
        }
    }
}