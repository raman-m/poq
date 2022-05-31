using Microsoft.AspNetCore.Mvc.ModelBinding;
using Poq.BackendApi.Models;

namespace Poq.BackendApi.Binders
{
    /// <summary>
    /// Model binder for the <see cref="MultiValueParam"/> collections to follow Swagger spec.
    /// <para>Array Swagger specification: <see href="https://swagger.io/docs/specification/2-0/describing-parameters/#array">Array and Multi-Value Parameters</see></para>
    /// </summary>
    public class MultiValueParamModelBinder : IModelBinder
    {
        public const char DefaultCollectionFormatSeparator = ',';

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

            var values = string.Empty;

            // Collection format is "multi": multiple parameters with the same name
            if (valueProviderResult.Length > 1)
            {
                // No need to parse. Enumerate from value provider
                var fastModel = new MultiValueParam(valueProviderResult);

                bindingContext.Result = ModelBindingResult.Success(fastModel);
                return Task.CompletedTask;
            }

            // Collection format is not "multi" because of single parameter
            values = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(values))
                return Task.CompletedTask;

            #region Business logic

            if (!DetectSeparator(values, out char? separator))
            {
                // Cannot detect values separator of Swagger collection format.
                // Let csv (default) collection format for successful fallback
                separator = DefaultCollectionFormatSeparator;
            }

            // Optimistic parsing. There is no internal type parsing, cause there is no sense to parse string to a string.
            if (!MultiValueParam.TryParse(values, separator.Value, out MultiValueParam collection))
            {
                bindingContext.ModelState.TryAddModelError(modelName, "[1] Optimistic parsing has failed.");
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

            // Swagger "collectionFormat" table with definitions
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
