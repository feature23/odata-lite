using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace F23.ODataLite.Internal
{
    internal static class QueryCollectionExtensions
    {
        public static bool HasParam(this IQueryCollection query, string param, out StringValues filterValue)
        {
            return query.TryGetValue(param, out filterValue) && !string.IsNullOrWhiteSpace(filterValue);
        }

        public static bool HasIntParam(this IQueryCollection query, string param, out int intValue)
        {
            intValue = 0;
            return query.HasParam(param, out var stringValues) && int.TryParse(stringValues, out intValue);
        }
    }
}
