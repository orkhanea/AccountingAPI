using System.ComponentModel.DataAnnotations;

namespace Accounting.ViewModel
{
    public class TaxModel
    {

        [Key]

        [Required]
        public string Name { get; set; }

        [Required]
        public double Persentage { get; set; }

        [Required]
        public bool Type { get; set; }

    }
}
