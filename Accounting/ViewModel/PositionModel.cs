using System.ComponentModel.DataAnnotations;

namespace Accounting.ViewModel
{
    public class PositionModel
    {
        [Required, MaxLength(250)]
        public string PositionName { get; set; }

        [Required]
        public double Salary { get; set; }

        [Required]
        public int CompanyId { get; set; }
    }
}
