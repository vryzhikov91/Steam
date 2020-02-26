using SteamGameParse.Data.Context;

namespace SteamGameParse.Data.UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SteamGameParseContext _context;

        public UnitOfWork(SteamGameParseContext context)
        {
            _context = context;
        }

        public bool Commit()
        {
            return _context.SaveChanges() > 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

    public interface IUnitOfWork
    {
        bool Commit();
    }
}