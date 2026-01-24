namespace CarConfigPROJECTmvc.ViewModels
{
    public class CarConfigurationComponentVm
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; } = "";
        public string ComponentName { get; set; } = "";
        public decimal Price { get; set; }
    }
}
