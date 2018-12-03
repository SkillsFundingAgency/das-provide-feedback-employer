using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.Services
{
    internal interface ISettingService
    {
        string Get(string parameterName);
    }
}
