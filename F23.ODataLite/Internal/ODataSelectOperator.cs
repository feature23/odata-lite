using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace F23.ODataLite.Internal
{
    internal static class ODataSelectOperator
    {
        public static IQueryable<object> Apply<T>(IQueryable<T> data, IEnumerable<PropertyInfo> properties, string parameter)
        {
            var selectList = parameter.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var selectedProperties = selectList
                .Join(properties, i => i.Trim(), i => i.Name, (i, p) => p, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var queryExpr = selectedProperties.Count == 1
                ? ApplySingleProperty(data, selectedProperties.Single())
                : ApplyAnonymousType(data, selectedProperties);

            return (IQueryable<object>)data.Provider.CreateQuery(queryExpr);
        }

        private static MethodCallExpression ApplySingleProperty<T>(IQueryable<T> data, PropertyInfo property)
        {
            var sourceItem = Expression.Parameter(typeof(T), "i");

            var selector = Expression.Lambda(
                Expression.Property(sourceItem, property.GetMethod),
                sourceItem
            );

            return Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Select),
                new Type[] { data.ElementType, property.PropertyType },
                data.Expression, selector
            );
        }

        private static MethodCallExpression ApplyAnonymousType<T>(IQueryable<T> data, IList<PropertyInfo> selectedProperties)
        {
            var sourceItem = Expression.Parameter(typeof(T), "i");
            var sourceProperties = selectedProperties.ToDictionary(p => p.Name);

            var anonymousType = AnonymousTypeFactory.CreateType(selectedProperties);

            var bindings = anonymousType.GetFields()
                .Select(p => Expression.Bind(p, Expression.Property(sourceItem, sourceProperties[p.Name])))
                .OfType<MemberBinding>();

            var selector = Expression.Lambda(
                Expression.MemberInit(
                    Expression.New(anonymousType.GetConstructor(Type.EmptyTypes)),
                    bindings
                ),
                sourceItem
            );

            return Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Select),
                new Type[] { data.ElementType, anonymousType },
                data.Expression, selector
            );
        }
    }
}
