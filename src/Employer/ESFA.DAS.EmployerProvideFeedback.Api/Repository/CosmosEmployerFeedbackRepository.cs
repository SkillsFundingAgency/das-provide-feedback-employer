namespace ESFA.DAS.EmployerProvideFeedback.Api.Repository
{
    using ESFA.DAS.EmployerProvideFeedback.Api.Dto;

    public class CosmosEmployerFeedbackRepository : TypedCosmosDbRepository<CosmosEmployerFeedbackRepository, EmployerFeedback>, IEmployerFeedbackRepository
    {
    }
}