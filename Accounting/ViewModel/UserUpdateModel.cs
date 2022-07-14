using System.ComponentModel.DataAnnotations;

namespace Accounting.ViewModel
{
    public class UserUpdateModel
    {
        [MaxLength(255), Required]
        public string Name { get; set; }

        [MaxLength(255), Required]
        public string Surname { get; set; }

        [Required, MaxLength(250)]
        public string Mail { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public DateTime EmploymentDate { get; set; }

        [Required]
        public DateTime BDate { get; set; }

        [Required]
        public bool Status { get; set; }

        [Required]
        public int PositionId { get; set; }

        [Required]
        public string RoleId { get; set; }
    }
}
