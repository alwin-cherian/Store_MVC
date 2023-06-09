using DressStore.DataAccess.Data;
using DressStore.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DressStore.DataAccess.Repository
{
    public class UserRepository<T> : IUserRepository<T> where T : class
    {

        private readonly ApplicationDbContest _db;

        public UserRepository(ApplicationDbContest db)
        {
            _db = db;
        }

        public void Block(T entity)
        {
            var user = _db.Users.Find(entity);
            if (user == null)
            {
                return;
            }
            throw new NotImplementedException();

        }

        public IEnumerable<T> GetAll(string? includeProperties = null)
        {
            var UserList = new List<T>();
            throw new NotImplementedException();
        }

        public void UnBlock(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
