using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounting.Model
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(250)]
        public string CompanyName { get; set; }

        [Required]
        public bool CompanyType { get; set; }

        [Required]
        public bool Status { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }

        public List<Transaction> Transactions { get; set; }

        public List<Transaction> ReceiverTransactions { get; set; }

    }
}
