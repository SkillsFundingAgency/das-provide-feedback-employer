namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    public interface IProviderAttribute
    {
        string Name { get; set; }

        int Value { get; set; }
    }
}