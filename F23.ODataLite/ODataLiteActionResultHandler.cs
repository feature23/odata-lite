using F23.Hateoas;
using F23.ODataLite.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace F23.ODataLite
{
    internal static class ODataLiteActionResultHandler
    {
        public static Task<IActionResult> HandleResult(IActionResult actionResult, IQueryCollection query)
        {
            return actionResult is ObjectResult ok ? HandleObjectResult(ok, query) : Task.FromResult(actionResult);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        private static async Task<IActionResult> HandleObjectResult(ObjectResult ok, IQueryCollection query)
        {
            bool isQueryable = ok.Value.IsQueryable(out var rawData);

            if (!isQueryable && !ok.Value.IsEnumerable(out rawData))
            {
                if (ok.Value is HypermediaResponse hypermedia)
                {
                    isQueryable = hypermedia.Content.IsQueryable(out rawData);

                    if (!isQueryable && !hypermedia.Content.IsEnumerable(out rawData))
                    {
                        throw new InvalidOperationException("HypermediaResponse.Content must be IQueryable<T> or IEnumerable<T> to use ODataLite. Pass a queryable or enumerable, or remove this attribute.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Data must be IQueryable<T> or IEnumerable<T> to use ODataLite. Pass a queryable or enumerable, or remove this attribute.");
                }
            }

            var itemType = rawData.GetType().GetGenericArguments().First();

            var applyMethod = typeof(ODataLiteActionResultHandler)
                .GetMethod(nameof(ApplyODataAsync), BindingFlags.Static | BindingFlags.NonPublic);

            Debug.Assert(applyMethod != null, nameof(applyMethod) + " != null");

            applyMethod = applyMethod.MakeGenericMethod(itemType);

            if (applyMethod.Invoke(null, new object[] { query, rawData, ok.Value as HypermediaResponse, isQueryable }) is Task<ObjectResult> result)
            {
                return await result;
            }

            return ok;
        }

        private static async Task<ObjectResult> ApplyODataAsync<T>(IQueryCollection query, IQueryable rawData, HypermediaResponse hypermediaResponse, bool isQueryable)
            where T : class
        {
            var data = (IQueryable<T>)rawData;

            var properties = new Lazy<PropertyInfo[]>(() => typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance));

            bool hasSelect = false;

            if (query.HasParam(ODataQueryParameters.Filter, out var filterValue))
            {
                data = ODataFilterOperator.Apply(
                    data, 
                    properties.Value, 
                    filterValue, 
                    isQueryable,
                    DetermineDecimalTypeSupport(data)
                );
            }

            if (query.HasParam(ODataQueryParameters.OrderBy, out var orderByValue))
            {
                data = ODataOrderByOperator.Apply(data, properties.Value, orderByValue);
            }

            IQueryable<object> projected;

            if (query.HasParam(ODataQueryParameters.Select, out var selectValue) && !selectValue.Equals("*"))
            {
                projected = ODataSelectOperator.Apply(data, properties.Value, selectValue);
                hasSelect = true;
            }
            else
            {
                projected = data.Cast<object>();
            }

            if (query.HasIntParam(ODataQueryParameters.Skip, out int skip))
            {
                projected = projected.Skip(skip);
            }

            if (query.HasIntParam(ODataQueryParameters.Top, out int top))
            {
                projected = projected.Take(top);
            }

            bool isAsync = projected.Provider is IAsyncQueryProvider;

            var result = query.ContainsKey(ODataQueryParameters.Count)
                ? CreateResult(isAsync ? await projected.CountAsync() : projected.Count(), hypermediaResponse)
                : CreateResult(isAsync ? await projected.ToListAsync() : projected.ToList(), hypermediaResponse);

            if (hasSelect)
            {
                result.Formatters.Add(SelectResultJsonFormatter.Instance);
            }

            return result;
        }

        private static bool DetermineDecimalTypeSupport<T>(IQueryable<T> data) where T : class
        {
            var isSqlite = data.GetDatabase()?.IsSqlite() == true;

            // TODO.JB - Detect other database providers?

            return !isSqlite;
        }

        private static OkObjectResult CreateResult(object value, HypermediaResponse hypermediaResponse)
        {
            var resultValue = value;

            if (hypermediaResponse != null)
            {
                resultValue = new HypermediaResponse(value)
                {
                    Links = hypermediaResponse.Links
                };
            }

            return new OkObjectResult(resultValue);
        }
    }
}
