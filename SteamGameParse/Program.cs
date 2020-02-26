using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using SteamGameParse.Data.Context;
using SteamGameParse.Data.Models;
using SteamGameParse.Data.Repository;
using SteamGameParse.Data.UoW;
using SteamGameParse.IoC;
using SteamGameParse.Scrapper;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = NLog.LogLevel;

namespace SteamGameParse
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddLogging(loggingBuilder =>
                {
                    // configure Logging with NLog
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    loggingBuilder.AddNLog( Path.Combine( Environment.CurrentDirectory, "nlog.config"));
                });
            NativeInjectorBootStrapper.RegisterServices(services);
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<Program>>();
        
            var genreRepository = serviceProvider.GetService<IGenreRepository>();
            var appDataRepository = serviceProvider.GetService<IAppDataRepository>();
            var searchAppRepository = serviceProvider.GetService<ISearchAppRepository>();
            var uOW = serviceProvider.GetService<IUnitOfWork>();
            serviceProvider.GetRequiredService<SteamGameParseContext>().Database.Migrate();

            logger.LogInformation( "Starting application");
            var service = new ScraperService(
                logger,
                genreRepository,
                appDataRepository,
                searchAppRepository,
                uOW
            );
            logger.LogInformation("start app search");
    
            // await service.SearchGamesAsync();
            var games = searchAppRepository.GetAll().Where(x => !x.ExecutionResult && x.Id < 15).ToList();
            foreach (var game in games)
            {
                logger.LogInformation($"Scraping app {game.Name}");
                await service.ScrapeAsync(game.Appid);
                var searchGame = searchAppRepository.GetByAppId(game.Appid);
                searchGame.UploadResult(true);
                searchAppRepository.SaveChanges();
                uOW.Commit();


            }
            
            logger.LogInformation("Scraping done");
        }
    }
}