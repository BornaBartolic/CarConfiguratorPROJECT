using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CarConfigPROJECTmvc.ViewModels.CarConfiguration
{
    public class ConfigStepVm
    {
        public int StepIndex { get; set; }
        public int ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; } = "";

        [Required(ErrorMessage = "U most choose 1 option")]
        public int? SelectedComponentId { get; set; }

        public List<CarConfigurationOptionsVM> Options { get; set; } = new();

        public List<ChosenItemVm> ChosenSoFar { get; set; } = new();
    }
}
