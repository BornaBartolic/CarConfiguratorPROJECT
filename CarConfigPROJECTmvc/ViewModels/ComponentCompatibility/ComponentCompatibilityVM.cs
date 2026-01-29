using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarConfigPROJECTmvc.ViewModels.ComponentCompatibility
{
    public class ComponentCompatibilityVM
    {
        public int Id { get; set; } // za Edit

        [Required(ErrorMessage = "Choose car type!")]
        [Display(Name = "Component 1")]
        public int CarComponentId1 { get; set; }

        [Required(ErrorMessage = "Choose component!")]
        [Display(Name = "Component 2")]
        public int CarComponentId2 { get; set; }

        // Ovo je bitno za dropdown
        public IEnumerable<SelectListItem> Components { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CarTypes { get; set; } = new();

    }
}