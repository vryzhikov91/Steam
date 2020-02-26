using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SteamGameParse.Scrapper.Interfaces
{
    public interface IStoreScraper
    {
        string GetIdFromUrl(string url);
        string GetUrlFromId(string id);
        Task<AppMetadata> ScrapeAsync(string appId);
        Task<AppIcon> DownloadIconAsync(AppMetadata meta);

        Action<TraceLevel, string, Exception> LogWritter { get; set; }
    }
}