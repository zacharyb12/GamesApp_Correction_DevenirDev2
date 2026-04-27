using GamesApp_Correction_DevenirDev2.Models;

namespace GamesApp_Correction_DevenirDev2.Repositories.UserRepositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserById(string id);

        Task<bool> UpdateUser();

        Task<bool> DeleteUser(string id);
    }
}
