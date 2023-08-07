using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OrderFlowAPI.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }
        public string orderInformation { get; set; }
        public DateTime OrderDate { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public ICollection<Item> Items { get; set; }
    }

    public class orderInput
    {
        public string orderInformation { get; set; }
        public int CustomerId { get; set; }

        public ICollection<Item> Items { get; set; }
    }

}
