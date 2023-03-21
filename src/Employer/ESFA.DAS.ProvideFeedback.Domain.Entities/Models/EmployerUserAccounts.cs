using System.Collections.Generic;
using System.Linq;
using ESFA.DAS.ProvideFeedback.Domain.Entities.ApiTypes;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Models
{
    public class EmployerUserAccounts
    {
        public IEnumerable<EmployerUserAccountItem> UserAccounts { get ; set ; }
        public string EmployerUserId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public bool IsSuspended { get; set; }

        public static implicit operator EmployerUserAccounts(GetUserAccountsResponse source)
        {
            if (source == null)
            {
                return null;
            }
            
            var userAccounts = new List<EmployerUserAccountItem>();
            if (source?.UserAccounts != null)
            {
                userAccounts = source.UserAccounts.Select(c => (EmployerUserAccountItem) c).ToList();
            }
            
            return new EmployerUserAccounts
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                EmployerUserId = source.EmployerUserId,
                UserAccounts = userAccounts,
                IsSuspended = source.IsSuspended
            };
        }
    }

    public class EmployerUserAccountItem
    {
        public string AccountId { get; set; }
        public string EmployerName { get; set; }
        public string Role { get; set; }
        
        public static implicit operator EmployerUserAccountItem(EmployerIdentifier source)
        {
            return new EmployerUserAccountItem
            {
                AccountId = source.AccountId,
                EmployerName = source.EmployerName,
                Role = source.Role
            };
        }
    }
}