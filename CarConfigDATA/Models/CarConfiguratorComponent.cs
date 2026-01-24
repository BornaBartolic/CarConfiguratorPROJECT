using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarConfigDATA.Models
{
    public class CarConfigurationComponent
    {
        public int CarConfigurationId { get; set; }
        public CarConfiguration CarConfiguration { get; set; } = null!;

        public int CarComponentId { get; set; }
        public CarComponent CarComponent { get; set; } = null!;
    }
}
