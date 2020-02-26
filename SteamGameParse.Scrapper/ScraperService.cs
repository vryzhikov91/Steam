#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SteamGameParse.Data.Models;
using SteamGameParse.Data.Repository;
using SteamGameParse.Data.UoW;
using SteamGameParse.Scrapper.Util;

namespace SteamGameParse.Scrapper
{
    public class ScraperService
    {
        protected const string StoreUrlTemplate = "http://store.steampowered.com/api/appdetails?appids={0}";
        protected const string StoreUrlTemplate2 = "http://store.steampowered.com/app/{0}/";
        protected const string DescriptionRegex = "<meta.+?\"description\".+?content\\s?=\\s?\"(.+?)\"";
        protected const string StoreAppsTemplate = "https://api.steampowered.com/ISteamApps/GetAppList/v2/";
        protected readonly HttpClient Client;
        const string path = "files";

        protected string StoreUrlUserTemplate { get; } = "http://store.steampowered.com/app/{0}/";
        protected string IdFromUrlRegex { get; } = @"^http.*?://store\.steampowered\.com/app/([\d]+)";

        private const string DefaultUserAgent =
            "Mozilla/5.0 (Windows NT 10.0; AppStoresScraper/1.2; https://github.com/Dissimilis/AppStoresScraper)";

        private readonly ILogger _loger;
        private readonly IGenreRepository _genresRepository;
        private readonly IAppDataRepository _appDatRepository;
        private readonly ISearchAppRepository _searchDataRepository;
        private readonly IUnitOfWork _uow;

        public ScraperService(ILogger loger,
            IGenreRepository genresRepository,
            IAppDataRepository appDatRepository,
            ISearchAppRepository searchDataRepository,
            IUnitOfWork uow
        )
        {
            _loger = loger;
            _genresRepository = genresRepository;
            _appDatRepository = appDatRepository;
            _searchDataRepository = searchDataRepository;
            _uow = uow;
            Client = GenereteClient();
        }


        public async Task SearchGamesAsync()
        {
            var url = StoreAppsTemplate;

            var msg = new HttpRequestMessage(HttpMethod.Get, url);
            msg.Headers.Add("Accept", "text/json");
            var response = await Client.SendAsync(msg);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var json = JsonConvert.DeserializeObject<dynamic>(content);

            dynamic result = json["applist"].apps;

            foreach (var appJson in result)
            {
                var app = new SearchApp(appJson.name.ToString(), appJson.appid.ToString());
                if (_searchDataRepository.GetByAppId(app.Appid) == null)
                {
                    _searchDataRepository.Add(app);
                    _searchDataRepository.SaveChanges();
                    _uow.Commit();
                }
            }

            var games = _searchDataRepository.GetAll();
            _loger.LogInformation($"Saved games {games.Count()}");
        }

        public async Task ScrapeAsync(string appId)
        {
            var url = string.Format(StoreUrlTemplate, appId);
            var url2 = string.Format(StoreUrlTemplate2, appId);
            var msg = new HttpRequestMessage(HttpMethod.Get, url);
            msg.Headers.Add("Accept", "text/json");
            var response = await Client.SendAsync(msg);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var json = JsonConvert.DeserializeObject<dynamic>(content);
            if (json == null || json[appId]["success"] != true)
            {
                _loger?.Log(LogLevel.Warning, $"Steam url [{url}] returned unsuccessfull status (app not found)");
                return;
            }

            dynamic result = json[appId].data;
            var data = new AppData();
            data.Name = result.name;
            var headerUrl = result.header_image;

            var wc = new System.Net.WebClient();
            var fileName = $"{path}/{result.steam_appid}.jpg";
            var fullPath = Path.Combine(Environment.CurrentDirectory, fileName);
            wc.DownloadFile(headerUrl.ToString(), fullPath);

            var doc = new HtmlDocument();
            var hasWindowsReq = result.platforms?.windows;
            if (hasWindowsReq != null && (bool) hasWindowsReq)
            {
                try
                {
                    var hasMinimum = result.pc_requirements?.minimum;
                    if (hasMinimum != null)
                    {
                        doc.LoadHtml(result.pc_requirements?.minimum.ToString());
                        var minHtml = doc.DocumentNode.Descendants("li")
                            .Select(x => x)
                            .ToList();
                        if (minHtml.Any())
                        {
                            var req = SystemRequirementses(minHtml, RequirementsType.Minimum, SystemType.windows);
                            data.Requirements.Add(req);
                        }
                    }

                    var hasRecomedeted = result.pc_requirements?.recommended;
                    if (hasRecomedeted != null)
                    {
                        doc.LoadHtml(result.pc_requirements?.recommended?.ToString());

                        var maxHtml = doc.DocumentNode.Descendants("li")
                            .Select(x => x)
                            .ToList();
                        if (maxHtml.Any())
                        {
                            var req = SystemRequirementses(maxHtml, RequirementsType.Recommended, SystemType.windows);
                            data.Requirements.Add(req);
                        }
                    }
                }
                catch (Exception e)
                {
                    _loger.LogWarning(e.Message);
                }
                
            }

            var hasMacOsReq = result.platforms?.mac;
            if (hasMacOsReq != null && (bool) hasMacOsReq)
            {
                try
                {
                    doc.LoadHtml(result.mac_requirements?.minimum?.ToString());
                    var minHtml = doc.DocumentNode.Descendants("li")
                        .Select(x => x)
                        .ToList();
                    if (minHtml.Any())
                    {
                        var req = SystemRequirementses(minHtml, RequirementsType.Minimum, SystemType.macOs);
                        data.Requirements.Add(req);
                    }

                    doc.LoadHtml(result.mac_requirements?.recommended?.ToString());
                    var reqHtml = doc.DocumentNode.Descendants("li")
                        .Select(x => x)
                        .ToList();
                    if (reqHtml.Any())
                    {
                        var req = SystemRequirementses(reqHtml, RequirementsType.Recommended, SystemType.macOs);
                        data.Requirements.Add(req);
                    }
                }
                catch (Exception e)
                {
                    _loger.LogWarning(e.Message);
                }
            }

            var hasLinuxOsReq = result.platforms?.linux;
            if (hasLinuxOsReq != null && (bool) hasLinuxOsReq)
            {
                try
                {
                    doc.LoadHtml(result.linux_requirements?.minimum?.ToString());
                    var minHtml = doc.DocumentNode.Descendants("li")
                        .Select(x => x)
                        .ToList();
                    if (minHtml.Any())
                    {
                        var req = SystemRequirementses(minHtml, RequirementsType.Minimum, SystemType.linux);
                        data.Requirements.Add(req);
                    }

                    doc.LoadHtml(result.linux_requirements?.recommended?.ToString());
                    var recHtml = doc.DocumentNode.Descendants("li")
                        .Select(x => x)
                        .ToList();
                    if (recHtml.Any())
                    {
                        var req = SystemRequirementses(recHtml, RequirementsType.Recommended, SystemType.linux);
                        data.Requirements.Add(req);
                    }
                }

                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            foreach (var item in result.genres)
            {
                var genre = _genresRepository.GetByName(item.description?.ToString());
                if (genre == null)
                {
                    genre = new Genre()
                    {
                        Name = item.description
                    };
                    _genresRepository.Add(genre);
                    _genresRepository.SaveChanges();
                }

                data.Genres.Add(genre);
            }

            data.Name = result.name;
            data.Published = result.publishers[0]?.ToString();
            data.Developer = result.developers[0]?.ToString();
            data.Description = Utils.StripHtml(result.about_the_game);
            data.ReleaseDate = result.release_date.date;
            data.ImagePath = fileName;
            _appDatRepository.Add(data);
            var searchApp = _searchDataRepository.GetByAppId(appId);
            _uow.Commit();
        }

        private static SystemRequirementses SystemRequirementses(List<HtmlNode> reqHtml, RequirementsType rType,
            SystemType sType)
        {
            var req = new SystemRequirementses {RequirementsType = rType, SystemType = sType};
            foreach (var item in reqHtml)
            {
                var paramName = item.Descendants("strong").Select(x => x).FirstOrDefault()?.InnerText;
                if (string.IsNullOrEmpty(paramName)) continue;
                var value = Utils.RemoveSubString(Utils.StripHtml(item.InnerText), paramName)
                    .Trim();

                if (string.IsNullOrEmpty(paramName)) continue;
                switch (paramName)
                {
                    case "OS:":
                        req.Os = value;
                        break;
                    case "Processor:":
                        req.Processor = value;
                        break;
                        ;
                    case "Memory:":
                        req.Memory = value;
                        break;
                    case "Graphics:":
                        req.Graphics = value;
                        break;
                    case "DirectX:":
                        req.DirectX = value;
                        break;
                    case "Storage:":
                        req.Storage = value;
                        break;
                }
            }

            return req;
        }

        public static HttpClient GenereteClient()
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() {CookieContainer = cookieContainer};
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var client = new HttpClient(handler);

            client.DefaultRequestHeaders.Add("User-Agent", DefaultUserAgent);
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.6");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            return client;
        }
    }
}