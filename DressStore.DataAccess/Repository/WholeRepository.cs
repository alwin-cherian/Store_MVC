﻿using DressStore.DataAccess.Data;
using DressStore.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DressStore.DataAccess.Repository
{
    public class WholeRepository : IWholeRepository
    {
        private readonly ApplicationDbContest _db;

        public ICategoryRepository Category { get; private set; }
        public IProductRepository product { get; private set; }

        public IApplicationUserRepository applicationUser {get; private set;}
        public IShoppingCartRepository shoppingCart { get; private set; }

        public IOrderDetailRepository orderDetail { get; private set; }
        public IOrderHeaderRepository orderHeader { get; private set; }

        public ICouponRepository coupon { get; private set; }

        public WholeRepository(ApplicationDbContest db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            product = new ProductRepository(_db);
            shoppingCart = new ShoppingCartRepository(_db);
            applicationUser = new ApplicationUserRepository(_db);
            orderDetail = new OrderDetailRepository(_db);
            orderHeader = new OrderHeaderRepository(_db);
            coupon = new CouponRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
