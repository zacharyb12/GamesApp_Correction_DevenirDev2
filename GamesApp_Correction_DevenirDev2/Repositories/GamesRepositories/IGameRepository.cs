using GamesApp_Correction_DevenirDev2.Models;

namespace GamesApp_Correction_DevenirDev2.Repositories.GamesRepositories
{
    public interface IGameRepository
    {
        Task<IEnumerable<Game>> GetGames();
        Task<IEnumerable<Game>> GetMyGames(string userId);
        Task<Game?> GetGameById(int id);
        Task<bool> CreateGame(CreateGameDTOs newGame,string userId);
        Task<bool> UpdateGame(UpdateGameDTOs updatedGame);
        Task<bool> DeleteGame(int id);

    }
}
