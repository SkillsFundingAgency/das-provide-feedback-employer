using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Das.ProvideFeedback.Domain.Entities;
using ESFA.DAS.ProvideFeedback.Data;

namespace Esfa.Das.Feedback.Employer.Emailer
{
    public class StubEmailDetailsStore : IStoreEmployerEmailDetails
    {
        public Task<EmployerEmailDetail> GetEmailDetailsForUniqueCode(Guid guid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<EmployerEmailDetail>> GetEmailDetailsToBeSent(int amount)
        {
            IEnumerable<EmployerEmailDetail> list = new List<EmployerEmailDetail>
            {
                new EmployerEmailDetail
                {
                    UserRef = new Guid("7f11a6b0-a25b-45a5-bdfc-4424dfba85e8"),
                    EmailAddress = "email@email.com",
                    EmailCode = new Guid(),
                    ProviderName = "My Provider",
                    UserFirstName = "Robert"
                }
            };

            return Task.FromResult(list);
        }

        public Task<bool> IsCodeBurnt(Guid emailCode)
        {
            throw new NotImplementedException();
        }

        public Task SetCodeBurntDate(DateTime codeBurntDate)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailDetailsAsSent(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailDetailsAsSent(IEnumerable<Guid> id)
        {
            throw new NotImplementedException();
        }
    }
}