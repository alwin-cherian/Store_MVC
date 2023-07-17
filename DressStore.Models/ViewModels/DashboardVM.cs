using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DressStore.Models.ViewModels
{
    public class DashboardVM
    {
        public Product Product { get; set; }

        public OrderHeader OrderHeader { get; set; }

        public OrderDetail OrderDetail { get; set; }

    }
}
