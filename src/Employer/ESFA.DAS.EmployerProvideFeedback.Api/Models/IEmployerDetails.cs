namespace ESFA.DAS.EmployerProvideFeedback.Api.Dto
{
    using System;

    public interface IEmployerDetails
    {
        long AccountId { get; set; }

        Guid UserRef { get; set; }
    }
}