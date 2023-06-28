using DressStore.DataAccess.Data;
using DressStore.DataAccess.Repository.IRepository;
using DressStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DressStore.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser> , IApplicationUserRepository
    {
        private ApplicationDbContest _db;

        public ApplicationUserRepository(ApplicationDbContest db) : base(db)
        {
            _db = db;
        }

        public void Update(ApplicationUser obj)
        {
            _db.ApplicationUsers.Update(obj);
        }
    }
}
