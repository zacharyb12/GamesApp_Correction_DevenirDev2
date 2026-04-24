using GamesApp_Correction_DevenirDev2.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GamesApp_Correction_DevenirDev2.Data
{
    public class GameContext : IdentityDbContext<User>
    {
        public GameContext(DbContextOptions<GameContext> options) : base(options)
        {
            
        }

        public DbSet<Game> Games { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(GameContext).Assembly);
        }
    }
}
