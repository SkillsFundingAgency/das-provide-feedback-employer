using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Das.Feedback.Employer.Emailer
{
    public class StubEmailDetailsStore : IStoreEmailDetails
    {
        public Task<IEnumerable<EmployerEmailDetail>> GetEmailDetailsToBeSent()
        {
            IEnumerable<EmployerEmailDetail> list = new List<EmployerEmailDetail>
            {
                new EmployerEmailDetail
                {
                    UserRef = new Guid("7f11a6b0-a25b-45a5-bdfc-4424dfba85e8"),
                    Email = "lee.wadhams@valtech.co.uk",
                    EmailCode = new Guid(),
                    ProviderName = "My Provider",
                    UserFirstName = "Robert"
                }
            };

            return Task.FromResult(list);
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