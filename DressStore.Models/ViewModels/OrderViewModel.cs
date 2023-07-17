using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DressStore.Models.ViewModels
{
    public class OrderViewModel
    {
        public IEnumerable<OrderHeader> orderHeaders {  get; set; }

        public IEnumerable<OrderDetail> OrderDetail { get; set; }
    }
}
