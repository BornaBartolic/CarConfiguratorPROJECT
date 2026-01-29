using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CarConfigPROJECTmvc.ViewModels.ComponentCompatibility
{
    public class ComponentCompatibilityEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please choose component!")]
        public int CarComponentId1 { get; set; }
        [Required(ErrorMessage = "Please choose component!")]
        public int CarComponentId2 { get; set; }

        public List<SelectListItem> Components { get; set; } = new();
    }
}
