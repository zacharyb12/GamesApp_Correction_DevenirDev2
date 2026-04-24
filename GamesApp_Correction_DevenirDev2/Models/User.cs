using Microsoft.AspNetCore.Identity;

namespace GamesApp_Correction_DevenirDev2.Models
{
    public class User : IdentityUser
    {
        public string Prenom { get; set; }
        public string Nom { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Game> MyGames { get; set; } = new();
    }
}
