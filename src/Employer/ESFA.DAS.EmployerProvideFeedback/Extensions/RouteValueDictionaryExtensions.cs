using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ESFA.DAS.EmployerProvideFeedback.Extensions
{
    public static class RouteValueDictionaryExtensions
    {
        public static IDictionary<string, string> ToStringDictionary(this RouteValueDictionary rvd)
        {
            return rvd.ToDictionary(kvp => kvp.Key, kvp => Convert.ToString(kvp.Value, CultureInfo.InvariantCulture));
        }
    }
}
