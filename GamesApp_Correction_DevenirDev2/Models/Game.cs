namespace GamesApp_Correction_DevenirDev2.Models
{
    public class Game
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Editor { get; set; }

        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? UserId { get; set; }

        public User? User { get; set; }
    }
}
