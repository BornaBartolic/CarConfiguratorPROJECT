using CarConfigDATA.Models;

namespace CarConfigPROJECTmvc.ViewModels.CarConfiguration
{
    public class CarConfigurationDetailsVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        public List<CarConfigurationComponentVm> Components { get; set; } = new();

        public decimal TotalPrice => Components.Sum(x => x.Price);
    }
}
