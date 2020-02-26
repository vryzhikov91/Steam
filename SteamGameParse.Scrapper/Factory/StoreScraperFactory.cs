using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SteamGameParse.Scrapper.Interfaces;

namespace SteamGameParse.Scrapper.Factory
{
    public class StoreScraperFactory
    {
        private readonly Action<TraceLevel, string, Exception> _logWritter;
        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; AppStoresScraper/1.2; https://github.com/Dissimilis/AppStoresScraper)";
        
        protected readonly Dictionary<Type, IStoreScraper> RegisteredScraperInstances = new Dictionary<Type, IStoreScraper>();

        protected virtual IReadOnlyCollection<IStoreScraper> RegisretedScrapers => RegisteredScraperInstances.Values.ToArray();


        public HttpClient HttpClient { get; private set; }

        
        public StoreScraperFactory(HttpClient httpClient = null, string userAgent = DefaultUserAgent, Action<TraceLevel, string, Exception> logWritter = null)
        {
            _logWritter = logWritter;
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            if (httpClient == null)
            {
                HttpClient = new HttpClient(handler);
                
                HttpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
                HttpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.6");
                HttpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                HttpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            }
            else
            {
                HttpClient = httpClient;
            }
            
            RegisterScraper(new SteamStoreScraper(HttpClient) { LogWritter = _logWritter });
            
        }


        public virtual async Task<StoreScrapeResult> ScrapeAsync(string url, bool downloadImages = false)
        {
            var scraper = GetScraper(url);
            var id = scraper?.GetIdFromUrl(url);
            if (scraper == null)
                _logWritter?.Invoke(TraceLevel.Info, $"No scraper registered for URL [{url}]", null);
            else if (id == null)
                _logWritter?.Invoke(TraceLevel.Info, $"Scraper [{scraper.GetType().Name}] can't get app ID from URL [{url}]", null);
            return await ScrapeAsync(scraper, id, downloadImages);
        }


        public virtual async Task<StoreScrapeResult> ScrapeAsync(Type scraperType, string appId, bool downloadImages = true)
        {
            var scraper = GetScraper(scraperType);
            return await ScrapeAsync(scraper, appId, downloadImages);
        }
        
        public virtual async Task<StoreScrapeResult> ScrapeAsync(IStoreScraper scraper, string appId, bool downloadImages = true)
        {
            var sw = new Stopwatch();
            sw.Start();
            var result = new StoreScrapeResult();
            if (scraper == null)
            {
                return result;
            }
            result.ScraperType = scraper.GetType();
            result.AppId = appId;
            if (string.IsNullOrEmpty(appId))
                return result;
            try
            {
                result.Metadata = await scraper.ScrapeAsync(appId);
                if (result.Metadata != null)
                {
                    result.Metadata.ScraperType = result.ScraperType;
                    if (downloadImages)
                    {
                        if (result.Metadata.IconUrl != null && result.Metadata.IconUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        {
                            result.Icon = await scraper.DownloadIconAsync(result.Metadata);
                        }
                        else
                        {
                            _logWritter?.Invoke(TraceLevel.Warning, $"{scraper.GetType().Name} scraper did not found valid icon url [{result.Icon}]", null);
                        }
                    }
                    if (string.IsNullOrWhiteSpace(result.Metadata.Name))
                        result.Exception = new ScraperException(scraper.GetType().Name + " scraper failed to parse result", scraper, appId, result.Metadata.AppUrl);
                }
            }
            catch (Exception ex) //should probably only catch WebException
            {
                var wex = ex as WebException;
                var httpResponse = wex?.Response as HttpWebResponse;
                string url = null;
                int? statusCode = null;
                if (httpResponse != null)
                {
                    statusCode = (int)httpResponse.StatusCode;
                    url = httpResponse.ResponseUri.AbsoluteUri;
                }
                _logWritter?.Invoke(TraceLevel.Error, $"{scraper.GetType().Name} scraper threw an exception while scraping; {url}", ex);
                result.Exception = new ScraperException(scraper.GetType().Name + " scraper failed", scraper, ex, appId, url, statusCode);
            }
            result.ParseTime = sw.Elapsed;
            return result;
        }
        
        public virtual IStoreScraper GetScraper(string url)
        {
            if (string.IsNullOrEmpty(url) || RegisteredScraperInstances == null)
                return null;
            foreach (var (key, value) in RegisteredScraperInstances)
            {
                if (!string.IsNullOrWhiteSpace(value?.GetIdFromUrl(url)))
                {
                    return value;
                }
            }
            return null;
        }
        
        public virtual IStoreScraper GetScraper<T>() where T:class, IStoreScraper
        {
            RegisteredScraperInstances.TryGetValue(typeof(T), out var scraper);
            if (scraper == null)
                _logWritter?.Invoke(TraceLevel.Warning, $"Scraper {typeof(T).Name} is not registered", null);
            return scraper;
        }
        
        public virtual IStoreScraper GetScraper(Type scraperType)
        {
            if (scraperType == null)
                throw new ArgumentNullException(nameof(scraperType));
            RegisteredScraperInstances.TryGetValue(scraperType, out var scraper);
            if (scraper == null)
                _logWritter?.Invoke(TraceLevel.Warning, $"Scraper {scraperType.Name} is not registered", null);
            return scraper;
        }
        
        public virtual AppIdentification ParseUrl(string url)
        {
            var scraper = GetScraper(url);
            var id = scraper?.GetIdFromUrl(url);
            return string.IsNullOrEmpty(id) ? null : new AppIdentification() { Id = id, AppUrl = scraper.GetUrlFromId(id), ScraperType = scraper.GetType() };
        }


        public virtual string GetNormalizedUrl<T>(string appId) where T: class, IStoreScraper
        {
            var scraper = GetScraper<T>();
            return scraper?.GetUrlFromId(appId);
        }
        
        public string GetNormalizedUrl(Type scraperType, string appId)
        {
            var scraper = GetScraper(scraperType);
            return scraper?.GetUrlFromId(appId);
        }


        public void RegisterScraper(IStoreScraper scraper)
        {
            RegisteredScraperInstances[scraper.GetType()] = scraper;
        }
        
        public bool RemoveScraper<T> () where T: class, IStoreScraper
        {
            return RegisteredScraperInstances.Remove(typeof(T));
        }

        
    }
}