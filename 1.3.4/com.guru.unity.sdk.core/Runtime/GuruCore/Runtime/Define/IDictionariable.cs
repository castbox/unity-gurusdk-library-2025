
namespace Guru
{
    using System.Collections.Generic;

    public interface IDictionariable
    {
        Dictionary<string, object> ToDictionary();
    }
}