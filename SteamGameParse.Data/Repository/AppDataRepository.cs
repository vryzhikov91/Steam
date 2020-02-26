using SteamGameParse.Data.Context;
using SteamGameParse.Data.Models;

namespace SteamGameParse.Data.Repository
{
    public class AppDataRepository : Repository<AppData>, IAppDataRepository
    {
        public AppDataRepository(SteamGameParseContext context)
            : base(context)
        {
        }
    }
}