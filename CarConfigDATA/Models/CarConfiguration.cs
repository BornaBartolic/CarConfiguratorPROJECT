using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarConfigDATA.Models
{
    public class CarConfiguration
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!; 

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<CarConfigurationComponent> CarConfigurationComponents { get; set; } = new List<CarConfigurationComponent>();
    }

}
