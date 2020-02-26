using SteamGameParse.Data.Models;

namespace SteamGameParse.Data.Repository
{
    public interface IGenreRepository : IRepository<Genre>
    {
        Genre GetByName(string name);
    }
}