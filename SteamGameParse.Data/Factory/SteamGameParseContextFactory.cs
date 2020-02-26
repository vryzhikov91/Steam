using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SteamGameParse.Data.Context;

namespace SteamGameParse.Data.Factory
{
    public class SteamGameParseContextFactory
    {
        public class VanguardContextFactory : IDesignTimeDbContextFactory<SteamGameParseContext>
        {
            public SteamGameParseContext CreateDbContext(string[] args)
            {
                var optionsBuilder = new DbContextOptionsBuilder<SteamGameParseContext>();
                optionsBuilder.UseSqlServer(
                    "Server=.\\sqlexpress;Database=Steam;Trusted_Connection=false; User ID=sa;Password=MAdler01;");

                return new SteamGameParseContext(optionsBuilder.Options);
            }
        }
    }
}