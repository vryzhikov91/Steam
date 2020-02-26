using SteamGameParse.Data.Models;

namespace SteamGameParse.Data.Repository
{
    public interface ISearchAppRepository : IRepository<SearchApp>
    {
        SearchApp GetByAppId(string app);
    }
}