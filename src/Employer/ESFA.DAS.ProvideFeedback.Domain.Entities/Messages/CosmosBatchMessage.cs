using System;
using System.Collections.Generic;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Messages
{
    public class CosmosBatchMessage
    {
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
