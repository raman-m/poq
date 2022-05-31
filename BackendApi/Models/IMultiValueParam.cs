using System.Collections;

namespace Poq.BackendApi.Models
{
    public interface IMultiValueParam : ICollection<string>
    {
        void Parse(string source, char separator);
    }
}
