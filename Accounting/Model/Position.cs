using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounting.Model
{
    public class Position
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(250)]
        public string PositionName { get; set; }

        [Required]
        public bool Status { get; set; }

        [Required]
        public double Salary { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
