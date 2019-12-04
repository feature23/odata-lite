using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace F23.ODataLite.Internal
{
    internal static class ODataFilterOperator
    {
        public static IQueryable<T> Apply<T>(IQueryable<T> data, IEnumerable<PropertyInfo> properties, string parameter, bool isQueryable)
        {
            var parser = new UriQueryExpressionParser(10);

            var token = parser.ParseFilter(parameter);

            var param = Expression.Parameter(typeof(T));

            var visitor = new ExpressionQueryTokenVisitor(param, properties, !isQueryable);

            var expr = token.Accept(visitor);

            var lambda = Expression.Lambda<Func<T, bool>>(expr, param);

            return data.AsQueryable().Where(lambda);
        }
    }
}
