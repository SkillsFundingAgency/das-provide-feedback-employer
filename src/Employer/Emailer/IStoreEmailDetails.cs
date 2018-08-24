using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Das.Feedback.Employer.Emailer
{
    public interface IStoreEmailDetails
    {
        Task<IEnumerable<EmployerEmailDetail>> GetEmailDetailsToBeSent();
        Task SetEmailDetailsAsSent(Guid emailCode);
        Task SetEmailDetailsAsSent(IEnumerable<Guid> id);
    }
}