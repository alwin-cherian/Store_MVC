using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DressStore.DataAccess.Repository.IRepository
{
    public interface IUserRepository <T> where T : class
    {
        IEnumerable<T> GetAll(string? includeProperties = null);

        void Block(T entity);

        void UnBlock(T entity);
    }
}
