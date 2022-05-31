using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Poq.BackendApi.Models;

namespace Poq.BackendApi.Binders
{
    public class MultiValueParamModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(MultiValueParam))
                return new BinderTypeModelBinder(typeof(MultiValueParamModelBinder));

            return null;
        }
    }
}
