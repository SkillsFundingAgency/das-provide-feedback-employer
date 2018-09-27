using System;

namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    [Serializable]
    public class ProviderAttributeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Good { get; set; }
        public bool Bad { get; set; }
        public int Score => Good ? 1 : Bad ? -1 : 0;
    }
}
