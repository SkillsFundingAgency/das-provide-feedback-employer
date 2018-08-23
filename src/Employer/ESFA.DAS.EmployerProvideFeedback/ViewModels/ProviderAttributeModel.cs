using System;

namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    [Serializable]
    public class ProviderAttributeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDoingWell { get; set; }
        public bool IsToImprove { get; set; }
        public int Score => IsDoingWell ? 1 : IsToImprove ? -1 : 0;
    }
}
