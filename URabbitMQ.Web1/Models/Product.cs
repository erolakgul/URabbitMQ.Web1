using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace URabbitMQ.Web1.Models
{
    public class Product
    {
        [Key]
        public int ID { get; set; }
        [StringLength(100)]
        public string? Name { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal Price { get; set; }
        [Range(1,100)]
        public int AvailableStock { get; set; }
        [StringLength(100)]
        public string? ImageName { get; set; }
    }
}
