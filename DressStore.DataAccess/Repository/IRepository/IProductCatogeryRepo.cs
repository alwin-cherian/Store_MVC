using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DressStore.DataAccess.Repository.IRepository
{
    public interface IProductCatogeryRepo
    {
        ICategoryRepository Category { get; }
        
        IProductRepository product { get; }

        void Save();
    }
}
