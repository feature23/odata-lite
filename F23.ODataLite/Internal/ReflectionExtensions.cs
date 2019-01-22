using System;
using System.Linq.Expressions;
using System.Reflection;

namespace F23.ODataLite.Internal
{
    internal static class ReflectionExtensions
    {
        public static Expression<Func<T, object>> CreateExpression<T>(this PropertyInfo selectedProperty)
        {
            var param = Expression.Parameter(typeof(T));

            return Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Property(param, selectedProperty), typeof(object)), param);
        }
    }
}
