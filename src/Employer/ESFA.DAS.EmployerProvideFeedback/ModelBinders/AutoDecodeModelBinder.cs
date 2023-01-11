using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.Encoding;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ESFA.DAS.EmployerProvideFeedback.Attributes.ModelBinders
{
    public class AutoDecodeModelBinder : IModelBinder
    {
        private readonly IModelBinder _fallbackBinder;
        private readonly IEncodingService _encodingService;

        public AutoDecodeModelBinder(IModelBinder fallbackBinder, IEncodingService encodingService)
        {
            _encodingService = encodingService;
            _fallbackBinder = fallbackBinder;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var attribute = GetAutoDecodeAttribute(bindingContext);

            if (attribute == null)
            {
                return _fallbackBinder.BindModelAsync(bindingContext);
            }

            try
            {
                var encodedValue = bindingContext.ValueProvider.GetValue(attribute.Source).FirstValue;

                if (GetTargetType(bindingContext) == typeof(long))
                {
                    var decodedValue = _encodingService.Decode(encodedValue, attribute.EncodingType);
                    bindingContext.Result = ModelBindingResult.Success(decodedValue);
                }
                else if (GetTargetType(bindingContext) == typeof(int))
                {
                    var decodedValue = (int)_encodingService.Decode(encodedValue, attribute.EncodingType);
                    bindingContext.Result = ModelBindingResult.Success(decodedValue);
                }
                else
                {
                    bindingContext.Result = ModelBindingResult.Failed();
                }
            }
            catch
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }

        private AutoDecodeAttribute GetAutoDecodeAttribute(ModelBindingContext context)
        {
            var container = context.ModelMetadata.ContainerType;
            if (container == null) return null;

            var propertyType = container.GetProperty(context.ModelMetadata.PropertyName);
            var attributes = propertyType.GetCustomAttributes(true);
            var attribute = (AutoDecodeAttribute)attributes.FirstOrDefault(a => a.GetType().IsEquivalentTo(typeof(AutoDecodeAttribute)));

            return attribute;
        }

        private Type GetTargetType(ModelBindingContext context)
        {
            var container = context.ModelMetadata.ContainerType;
            if (container == null) return null;
            var property = container.GetProperty(context.ModelMetadata.PropertyName);
            return property?.PropertyType;
        }
    }
}