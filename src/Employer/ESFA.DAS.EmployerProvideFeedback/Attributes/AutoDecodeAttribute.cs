using SFA.DAS.Encoding;
using System;

namespace ESFA.DAS.EmployerProvideFeedback.Attributes
{
    public class AutoDecodeAttribute : Attribute
    {
        public string Source { get; set; }
        public EncodingType EncodingType { get; set; }

        public AutoDecodeAttribute(string source, EncodingType encodingType)
        {
            Source = source;
            EncodingType = encodingType;
        }
    }
}
