using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounting.Model
{
    public class User : IdentityUser
    {
        [MaxLength(255), Required]
        public string Name { get; set; }

        [MaxLength(255), Required]
        public string Surname { get; set; }

        [MaxLength(250)]
        public string? Image { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime EmploymentDate { get; set; }

        public DateTime? DismissialDate { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public DateTime BDate { get; set; }

        public bool Status { get; set; }

        [ForeignKey("Position")]
        public int PositionId { get; set; }
        public Position Position { get; set; }

        
    }
}
