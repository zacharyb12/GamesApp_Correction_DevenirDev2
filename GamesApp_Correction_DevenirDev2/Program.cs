using GamesApp_Correction_DevenirDev2.Data;
using GamesApp_Correction_DevenirDev2.Models;
using GamesApp_Correction_DevenirDev2.Repositories.GamesRepositories;
using GamesApp_Correction_DevenirDev2.Repositories.UserRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GamesApp_Correction_DevenirDev2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configuration Entity
            builder.Services.AddDbContext<GameContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("myConnection"))
            );

            // Configuration Injection de dépendances
            builder.Services.AddScoped<IGameRepository,GameRepository>();
            builder.Services.AddScoped<IUserRepository , UserRepository>();


            // Configuration de l'identity
            builder.Services.AddIdentity<User, IdentityRole>(options =>
             {
                 options.Password.RequiredLength = 5;
                 options.Password.RequireDigit = true;
                 options.Password.RequireUppercase = true;
                 //options.Password.RequireLowercase = true;
                 //options.Password.RequireNonAlphanumeric = false;

                 //// Verrouillage après échecs
                 //options.Lockout.MaxFailedAccessAttempts = 5;
                 //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
             }
            ).AddEntityFrameworkStores<GameContext>()
            .AddDefaultTokenProviders();


            // Configuration du cookie
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Home/Index";
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.SlidingExpiration = true;

            }
            );

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
