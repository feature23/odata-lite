using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace F23.ODataLite.Internal
{
    internal static class ODataOrderByOperator
    {
        public static IQueryable<T> Apply<T>(IQueryable<T> data, IEnumerable<PropertyInfo> properties, string parameter)
        {
            var orderByList = parameter.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var selectedProperties = orderByList
                .Select(i => new OrderByExpression(i))
                .Join(properties, i => i.Property, i => i.Name, (i, p) => new { Property = p, i.Descending }, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (selectedProperties.Count == 0)
            {
                return data;
            }

            var queryable = selectedProperties[0].Descending
                ? data.AsQueryable().OrderByDescending(selectedProperties[0].Property.CreateExpression<T>())
                : data.AsQueryable().OrderBy(selectedProperties[0].Property.CreateExpression<T>());

            foreach (var property in selectedProperties.Skip(1))
            {
                queryable = property.Descending
                    ? queryable.ThenByDescending(property.Property.CreateExpression<T>())
                    : queryable.ThenBy(property.Property.CreateExpression<T>());
            }

            return queryable;
        }
    }
}
