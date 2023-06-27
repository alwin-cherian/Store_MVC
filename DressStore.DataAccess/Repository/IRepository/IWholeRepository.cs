using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DressStore.DataAccess.Repository.IRepository
{
    public interface IWholeRepository
    {
        ICategoryRepository Category { get; }
        
        IProductRepository product { get; }

        IApplicationUserRepository applicationUser { get; }

        IShoppingCartRepository shoppingCart { get; }

        IOrderHeaderRepository orderHeader { get; }

        IOrderDetailRepository orderDetail { get; }

        ICouponRepository coupon { get; }
        void Save();
    }
}
