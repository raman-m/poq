using System.Collections.Generic;
using System.Linq;

namespace Poq.BackendApi.Models
{
    /// <summary>
    /// Array-like collection object which compatible to Swagger multi-value parameter.
    /// <para>Swagger Docs: <see href="https://swagger.io/docs/specification/2-0/describing-parameters/#array">Array and Multi-Value Parameters</see></para>
    /// </summary>
    public class MultiValueParam : List<string>
    {
        protected MultiValueParam() { }

        public MultiValueParam(IEnumerable<string> collection)
            : base(collection) { }

        public void Parse(string source, char separator)
        {
            if (string.IsNullOrEmpty(source))
                return;

            Clear();

            if (!source.Contains(separator))
            {
                Add(source); // parsed successfully but there is one element
                return;
            }

            var elements = source
                .Split(separator)
                .Where(s => !string.IsNullOrEmpty(s));
            
            AddRange(elements);
        }

        public static bool TryParse(string source, char separator, out MultiValueParam result)
        {
            result = new MultiValueParam();

            if (string.IsNullOrEmpty(source))
                return false;

            result.Parse(source, separator);
            return true;
        }
    }
}
