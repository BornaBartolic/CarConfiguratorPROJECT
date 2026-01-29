namespace CarConfigPROJECTmvc.ViewModels.ComponentCompatibility
{
    public class ComponentCompatibilityRowVm
    {
        public int Id { get; set; }
        public int CarComponentId1 { get; set; }
        public int CarComponentId2 { get; set; }
        public string Component1Name { get; set; } = "";
        public string Component2Name { get; set; } = "";
    }
}
