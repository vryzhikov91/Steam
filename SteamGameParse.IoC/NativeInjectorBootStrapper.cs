using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SteamGameParse.Data.Context;
using SteamGameParse.Data.Models;
using SteamGameParse.Data.Repository;
using SteamGameParse.Data.UoW;

namespace SteamGameParse.IoC
{
    public class NativeInjectorBootStrapper
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<SteamGameParseContext>();
            services.AddScoped<ISearchAppRepository, SearchAppRepository>();
            services.AddScoped<IAppDataRepository, AppDataRepository>();
            services.AddScoped<IGenreRepository, GenreRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            services.AddDbContext<SteamGameParseContext>(options =>
                options.UseSqlServer("Server=.\\sqlexpress;Database=Steam;Trusted_Connection=false; User ID=sa;Password=MAdler01;"));
            services.AddScoped<SteamGameParseContext>();
        }
    }
}