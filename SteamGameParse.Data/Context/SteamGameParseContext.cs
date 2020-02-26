using Microsoft.EntityFrameworkCore;
using SteamGameParse.Data.Models;

namespace SteamGameParse.Data.Context
{
    public class SteamGameParseContext: DbContext
    {
        public SteamGameParseContext(DbContextOptions<SteamGameParseContext> options) : base(options)
        {
        }
        public DbSet<SearchApp> SearchApps { get; set; }
        public DbSet<AppData> AppData { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<SystemRequirementses> Requirements { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}