using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarConfigPROJECTmvc.ViewModels
{
    public class ComponentVM
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string ComponentTypeName { get; set; } = null!;

        public IEnumerable<SelectListItem>? ComponentTypeList { get; set; }
    }
}
