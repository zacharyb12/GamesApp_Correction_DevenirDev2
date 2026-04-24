using System.ComponentModel.DataAnnotations;

namespace GamesApp_Correction_DevenirDev2.Models
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Le prénom est requis")]
        public string Prenom { get; set; }


        [Required(ErrorMessage = "Le nom est requis")]
        public string Nom { get; set; }


        [Required(ErrorMessage = "L' email est requis")]
        public string Email { get; set; }


        public string Password { get; set; }

        [Compare("Password" , ErrorMessage = "Les mots de passe ne correspondent pas ")]
        public string ConfirmPassword { get; set; }
    }

}
