using System.ComponentModel.DataAnnotations.Schema;

namespace CarConfigDATA.Models
{
    public class CarComponentCompatibility
    {
        public int Id { get; set; }  

        public int CarComponentId1 { get; set; }
        public int CarComponentId2 { get; set; }

        [ForeignKey("CarComponentId1")]
        public CarComponent CarComponent1 { get; set; } = null!;

        [ForeignKey("CarComponentId2")]
        public CarComponent CarComponent2 { get; set; } = null!;
    }
}