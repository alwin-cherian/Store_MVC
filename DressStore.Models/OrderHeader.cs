using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DressStore.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        [ValidateNever] 
        public ApplicationUser ApplicationUser { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }
        public double OrderTotal { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CouponDiscount { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? NewOrderTotal { get; set; }

        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set;}

        public DateTime? PaymentDate { get; set; }
        public DateTime? DueDate { get; set; }

        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [StringLength(60, ErrorMessage = "The Address should be properly specified", MinimumLength = 6)]
        public string StreetAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        [StringLength(6,ErrorMessage ="PinCode must be 6 digits ",MinimumLength = 5)]
        public string PostalCode { get; set; }
        [Required]
        [StringLength(12, ErrorMessage = "The phone number must be 10-12 digits long.", MinimumLength = 6)]
        public string phoneNumber { get; set; }
        
    }
}
