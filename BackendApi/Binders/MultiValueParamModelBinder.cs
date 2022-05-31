using Microsoft.AspNetCore.Mvc.ModelBinding;
using Poq.BackendApi.Models;

namespace Poq.BackendApi.Binders
{
    public class MultiValueParamModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var values = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(values))
                return Task.CompletedTask;

            #region Business logic

            if (!DetectSeparator(values, out char? separator))
            {
                bindingContext.ModelState.TryAddModelError(modelName, "[1] Cannot detect values separator of Swagger collection format.");
                separator = ','; // assume csv (default) collection format for successful fallback
            }

            // Optimistic parsing. There is no internal type parsing, cause there is no sense to parse string to a string.
            if (!MultiValueParam.TryParse(values, separator.Value, out MultiValueParam collection))
            {
                bindingContext.ModelState.TryAddModelError(modelName, "[2] Optimistic parsing has failed.");
                return Task.CompletedTask;
            }

            // Success path
            //
            // {Call other extra logic applications to build the model.}
            //
            MultiValueParam model = collection;
            bindingContext.Result = ModelBindingResult.Success(model);

            #endregion

            return Task.CompletedTask;
        }

        private bool DetectSeparator(string source, out char? separator)
        {
            separator = null;

            if (string.IsNullOrEmpty(source))
                return false;

            // Swagger "collectionFormat" specification: https://swagger.io/docs/specification/2-0/describing-parameters/#array
            const string candidates = @", \|";

            foreach (char candidate in candidates)
            {
                if (source.Contains(candidate))
                {
                    separator = candidate;
                    return true;
                }
            }
            return false;
        }
    }
}
