using System.Linq;
using SteamGameParse.Data.Context;
using SteamGameParse.Data.Models;

namespace SteamGameParse.Data.Repository
{
    public class SearchAppRepository : Repository<SearchApp>, ISearchAppRepository
    {
        public SearchAppRepository(SteamGameParseContext context)
            : base(context)
        {
            
        }

        public SearchApp GetByAppId(string app)
        {
            return base.GetAll().FirstOrDefault(x => x.Appid == app);
        }
    }
}