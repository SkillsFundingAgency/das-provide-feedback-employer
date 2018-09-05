using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Das.ProvideFeedback.Domain.Entities;

namespace ESFA.DAS.ProvideFeedback.Data
{
    public interface IStoreEmployerEmailDetails
    {
        Task<IEnumerable<EmployerEmailDetail>> GetEmailDetailsToBeSent();
        Task SetEmailDetailsAsSent(Guid emailCode);
        Task SetEmailDetailsAsSent(IEnumerable<Guid> id);
        Task<EmployerEmailDetail> GetEmailDetailsForUniqueCode(Guid guid);
        Task SetCodeBurntDate(Guid uniqueCode);
        Task<bool> IsCodeBurnt(Guid emailCode);
    }
}