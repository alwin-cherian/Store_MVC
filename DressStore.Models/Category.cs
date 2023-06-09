using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DressStore.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Category Name")]
        [MaxLength(30)]
        public string Name { get; set; }

        [Range(0, 30 , ErrorMessage ="Must be Between 1 - 30")]
        [DisplayName("Display Order")]
        public int DisplayOrder { get; set; }

        public bool IsAvailable { get; set; }
    }
}
