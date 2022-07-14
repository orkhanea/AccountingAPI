using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounting.Model
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Note { get; set; }

        [Required]
        public DateTime SendDate { get; set; }

        [Required]
        public DateTime RetiredDate { get; set; }

        public DateTime CreatedDate { get; set; }

        [Required]
        public double NETAmount { get; set; }
        
        [Required]
        public double TAXAmount { get; set; }

        public int SenderCompanyId { get; set; }
        public Company SenderCompany { get; set; }

        public int RecieverCompanyId { get; set; }
        public Company RecieverCompany { get; set; }
    }
}
