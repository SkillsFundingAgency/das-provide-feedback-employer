namespace ESFA.DAS.EmployerProvideFeedback.Api.Repository
{
    using ESFA.DAS.EmployerProvideFeedback.Api.Dto;

    public class CosmosTokenStore : TypedCosmosDbRepository<CosmosTokenStore, ApplicationKey>, ITokenProvider
    {
    }
}