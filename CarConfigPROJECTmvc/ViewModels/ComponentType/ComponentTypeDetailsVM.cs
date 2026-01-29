using CarConfigPROJECTmvc.ViewModels.Component;

namespace CarConfigPROJECTmvc.ViewModels.ComponentType
{
    public class ComponentTypeDetailsVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";

        public List<ComponentRowVm> Components { get; set; } = new();

        public decimal TotalPrice => Components.Sum(x => x.Price);
    }
}
