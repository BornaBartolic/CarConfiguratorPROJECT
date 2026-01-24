using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarConfigPROJECTmvc.ViewModels
{
    public class ComponentCompatibilityEditVM
    {
        public int Id { get; set; } 
        public int CarComponentId1 { get; set; }
        public int CarComponentId2 { get; set; }

        public List<SelectListItem> Components { get; set; } = new();
    }
}
