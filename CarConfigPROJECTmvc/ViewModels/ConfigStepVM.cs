using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarConfigPROJECTmvc.ViewModels
{
    public class ConfigStepVm
    {
        public int StepIndex { get; set; }
        public int ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; } = "";

        public int? SelectedComponentId { get; set; }

        public List<CarConfigurationOptionsVM> Options { get; set; } = new();

        public List<ChosenItemVm> ChosenSoFar { get; set; } = new();
    }
}
