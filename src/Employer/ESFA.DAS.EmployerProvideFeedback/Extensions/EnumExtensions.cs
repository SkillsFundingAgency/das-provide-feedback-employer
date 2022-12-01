using System;
using System.ComponentModel.DataAnnotations;

namespace ESFA.DAS.EmployerProvideFeedback.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumerationValue) //adhoc and email journeys
        {
            var type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException($"{nameof(enumerationValue)} must be of Enum type", nameof(enumerationValue));
            }
            var memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((DisplayAttribute)attrs[0]).Name;
                }
            }
            return enumerationValue.ToString();
        }
    }
}
