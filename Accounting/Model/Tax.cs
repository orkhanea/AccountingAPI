using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Accounting.Data;

namespace Accounting.Model
{
    public class Tax
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        public string Name { get; set; }

        public double? Persentage { get; set; }

        public double? Persentage2 { get; set; }

        [Required]
        public bool Type { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? Formula { get; set; }

        public double? Minimum { get; set; }

        public string? Formula2 { get; set; }
    }
}
