namespace CarConfigPROJECTmvc.ViewModels
{
    public class ComponentRowVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}
