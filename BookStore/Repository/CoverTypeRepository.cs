using BookStore.Data;
using BookStore.Models;
using BookStore.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Repository
{
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private readonly ApplicationDbContext _db;

        public CoverTypeRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
        }

        public void Update(CoverType covertType)
        {
            var objFromDb = _db.CoverTypes.FirstOrDefault(s => s.Id == covertType.Id);
            if(objFromDb != null)
            {
                objFromDb.Name = covertType.Name;
            }
        }

    }
}
