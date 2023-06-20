using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DressStore.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(30)]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Must be a postive value")]
        [Display(Name = "Price")]
        public double Price { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category category { get; set; }
        [ValidateNever]
        public string ImageUrl { get; set; }

        [Required]
        public double TotalQuantity { get; set; }
    }
}
