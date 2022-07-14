using System.ComponentModel.DataAnnotations;

namespace Accounting.ViewModel
{
    public class TransactionModel
    {
        [Required]
        public string Note { get; set; }

        [Required]
        public DateTime SendDate { get; set; }

        [Required]
        public DateTime RetiredDate { get; set; }

        [Required]
        public double NETAmount { get; set; }

        [Required]
        public double TAXAmount { get; set; }

        [Required]
        public int SenderCompanyId { get; set; }

        [Required]
        public int RecieverCompanyId { get; set; }
        
    }
}
