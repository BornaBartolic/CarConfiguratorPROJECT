using CarConfigDATA.Models;

namespace CarConfigPROJECTmvc.ViewModels
{
    public class ComponentDeleteVM
    {
        public CarComponent Component { get; set; } = null!;
        public List<CarComponentCompatibility> Compatibilities { get; set; } = new();
    }
}
