using System.Collections;
using System.Linq;
using System.Reflection;

namespace F23.ODataLite.Internal
{
    internal static class ObjectExtensions
    {
        private static readonly MethodInfo _asQueryableMethod = typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(i => i.Name == nameof(Queryable.AsQueryable) && i.IsGenericMethod && i.GetParameters().Length == 1);

        public static bool IsQueryable(this object obj, out IQueryable queryable)
        {
            if (obj is IQueryable q && q.GetType().IsGenericType)
            {
                queryable = q;
                return true;
            }

            queryable = null;
            return false;
        }

        public static bool IsEnumerable(this object obj, out IQueryable queryable)
        {
            if (obj is IEnumerable e && e.GetType().IsGenericType)
            {
                queryable = ConvertToQueryable(e);
                return true;
            }

            queryable = null;
            return false;
        }

        public static bool IsQueryableOrEnumerable(this object obj, out IQueryable queryable)
        {
            return obj.IsQueryable(out queryable) || obj.IsEnumerable(out queryable);
        }

        private static IQueryable ConvertToQueryable(IEnumerable enumerable)
        {
            var type = enumerable.GetType().GetGenericArguments().First();

            var asQueryable = _asQueryableMethod.MakeGenericMethod(type);

            return asQueryable.Invoke(null, new object[] {enumerable}) as IQueryable;
        }
    }
}