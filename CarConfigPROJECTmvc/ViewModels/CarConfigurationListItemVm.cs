namespace CarConfigPROJECTmvc.ViewModels
{
    public class CarConfigurationListItemVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
