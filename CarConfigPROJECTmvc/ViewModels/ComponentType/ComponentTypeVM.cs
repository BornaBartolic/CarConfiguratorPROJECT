using System.ComponentModel.DataAnnotations;

namespace CarConfigPROJECTmvc.ViewModels.ComponentType
{
    public class ComponentTypeVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The Name field is required!")]
        public string Name { get; set; } = null!;
    }
}
