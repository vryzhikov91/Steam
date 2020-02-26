using System;
using System.Linq;

namespace SteamGameParse.Data.Repository
{
    public interface IRepository<TEntity>
    {
        void Add(TEntity obj);
        TEntity GetById(Guid id);
        IQueryable<TEntity> GetAll();
        void Update(TEntity obj);
        void Remove(Guid id);
        int SaveChanges();
       
    }
}