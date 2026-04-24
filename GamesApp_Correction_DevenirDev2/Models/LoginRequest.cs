using System.ComponentModel.DataAnnotations;

namespace GamesApp_Correction_DevenirDev2.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage ="Vous devez fournir votre email !")]
        [EmailAddress]
        public string Email { get; set; }


        [Required(ErrorMessage ="Vous devez fournir votre mot de passe !")]
        [DataType(DataType.Password)]
        public string  Password { get; set; }


        [Display(Name ="Se souvenir de moi")]
        public bool RememberMe { get; set; }
    }
}
