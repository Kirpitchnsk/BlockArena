using BlockArena.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BlockArena.Common
{
    public static class Extensions
    {
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> list, T item)
        {
            return (list ?? new List<T>()).Concat(new List<T> { item });
        }

        public static T ConvertTo<T>(this JsonElement jsonElement)
        {
            return JsonSerializer.Deserialize<T>(jsonElement.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
