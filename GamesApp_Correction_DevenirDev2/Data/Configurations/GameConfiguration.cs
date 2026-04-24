using GamesApp_Correction_DevenirDev2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GamesApp_Correction_DevenirDev2.Data.Configurations
{
    public class GameConfiguration : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder.ToTable("Game");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Id)
                .ValueGeneratedOnAdd();

            builder.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(g => g.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(g => g.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(g => g.Price)
                .HasColumnType("decimal(10,2)");

            builder.Property(g => g.Editor)
                .HasMaxLength(50);

            builder.Property(g => g.UserId);

            builder.HasOne(g => g.User)
                .WithMany(u => u.MyGames)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
