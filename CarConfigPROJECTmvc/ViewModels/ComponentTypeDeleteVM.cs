namespace CarConfigPROJECTmvc.ViewModels
{
    public class ComponentTypeDeleteVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";

        public List<string> ComponentNames { get; set; } = new();
    }
}
