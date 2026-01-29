using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CarConfigPROJECTmvc.ViewModels.Component
{
    public class ComponentVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The Name field is required!")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "The Description field is required!")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "The Price field is required!")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "The ComponentTypeName field is required!")]
        public string ComponentTypeName { get; set; } = null!;

        public IEnumerable<SelectListItem>? ComponentTypeList { get; set; }
    }
}
