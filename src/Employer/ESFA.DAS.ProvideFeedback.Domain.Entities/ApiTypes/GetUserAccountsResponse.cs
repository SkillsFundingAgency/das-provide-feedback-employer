using System.Collections.Generic;
using Newtonsoft.Json;
using SFA.DAS.GovUK.Auth.Employer;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.ApiTypes
{
    public class GetUserAccountsResponse
    {
        [JsonProperty(PropertyName = "isSuspended")]
        public bool IsSuspended { get; set; }
        [JsonProperty(PropertyName = "employerUserId")]
        public string EmployerUserId { get; set; }
        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }
        [JsonProperty("userAccounts")]
        public List<EmployerIdentifier> UserAccounts { get; set; }
    }
    
    public class EmployerIdentifier
    {
        [JsonProperty("encodedAccountId")]
        public string AccountId { get; set; }
        [JsonProperty("dasAccountName")]
        public string EmployerName { get; set; }
        [JsonProperty("role")]
        public string Role { get; set; }
        [JsonProperty("apprenticeshipEmployerType")]
        public ApprenticeshipEmployerType ApprenticeshipEmployerType { get; set; }
    }
}