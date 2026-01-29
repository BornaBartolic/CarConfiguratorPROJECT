using CarConfigDATA.Models;

namespace CarConfigPROJECTmvc.ViewModels.Component
{
    public class ComponentDeleteVM
    {
        public CarComponent Component { get; set; } = null!;
        public List<CarComponentCompatibility> Compatibilities { get; set; } = new();
    }
}
