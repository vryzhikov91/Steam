using System.Linq;
using SteamGameParse.Data.Context;
using SteamGameParse.Data.Models;

namespace SteamGameParse.Data.Repository
{
    public class GenreRepository : Repository<Genre>, IGenreRepository
    {
        public GenreRepository(SteamGameParseContext context)
            : base(context)
        {
        }

        public Genre GetByName(string name)
        {
           return base.GetAll().FirstOrDefault(x => x.Name == name);
        }
    }
}