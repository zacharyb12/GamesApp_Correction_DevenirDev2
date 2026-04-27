using GamesApp_Correction_DevenirDev2.Data;
using GamesApp_Correction_DevenirDev2.Models;
using Microsoft.EntityFrameworkCore;

namespace GamesApp_Correction_DevenirDev2.Repositories.UserRepositories
{
    public class UserRepository(GameContext _context) : IUserRepository
    {
        public async Task<User?> GetUserById(string id)
        {
            var result =  await _context.Users
                                .Include(u => u.MyGames)
                                .FirstOrDefaultAsync(u => u.Id == id);

            return result;
        }

        public Task<bool> DeleteUser(string id)
        {
            throw new NotImplementedException();
        }


        public Task<bool> UpdateUser()
        {
            throw new NotImplementedException();
        }
    }
}
