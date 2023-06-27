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
    public class CouponRepository : Repository<Coupon> , ICouponRepository
    {
        private readonly ApplicationDbContest _db;

        public CouponRepository(ApplicationDbContest db) : base(db)
        {
            _db = db;
        }

        public void Update(Coupon item)
        {
            _db.Coupons.Update(item);
        }

    }
}
