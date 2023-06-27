using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DressStore.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CouponName { get; set; }
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public Decimal?  DiscountAmount { get; set; }
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public Decimal MinPurchase { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public Decimal? MaxPurchase { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public Decimal? DiscountPercentage { get; set; }
        [Required]
        public DateTime ExpirationDate { get; set; }

        public bool? IsValid { get; set; }

    }
}
