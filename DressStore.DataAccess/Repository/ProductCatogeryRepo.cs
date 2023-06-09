using DressStore.DataAccess.Data;
using DressStore.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DressStore.DataAccess.Repository
{
    public class ProductCatogeryRepo : IProductCatogeryRepo
    {
        private readonly ApplicationDbContest _db;
        public ICategoryRepository Category { get; private set; }
        public IProductRepository product { get; private set; }

        public ProductCatogeryRepo(ApplicationDbContest db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            product = new ProductRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
