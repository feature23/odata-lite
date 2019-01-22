using F23.ODataLite.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace F23.ODataLite
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ODataLiteAttribute : ResultFilterAttribute
    {
        private static readonly JsonOutputFormatter SelectFormatter = new JsonOutputFormatter(new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true
                }
            }
        }, ArrayPool<char>.Shared);

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // TODO.JB - Re-add hypermedia support?

            if (!(context.Result is OkObjectResult ok)
                || !(ok.Value is IQueryable rawData && rawData.GetType().IsGenericType))
            {
                await base.OnResultExecutionAsync(context, next);
                return;
            }

            var itemType = rawData.GetType().GetGenericArguments().First();

            var applyMethod = typeof(ODataLiteAttribute)
                .GetMethod(nameof(ApplyODataAsync), BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(itemType);

            var result = applyMethod.Invoke(null, new object[] { context, rawData }) as Task<OkObjectResult>;

            context.Result = await result;

            await base.OnResultExecutionAsync(context, next);
        }

        private static async Task<OkObjectResult> ApplyODataAsync<T>(ActionContext context, IQueryable rawData)
        {
            var data = (IQueryable<T>)rawData;

            var properties = new Lazy<PropertyInfo[]>(() => typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance));

            var query = context.HttpContext.Request.Query;
            bool hasSelect = false;

            if (query.HasParam("$filter", out var filterValue))
            {
                data = ODataFilterOperator.Apply(data, properties.Value, filterValue);
            }

            if (query.HasParam("$orderby", out var orderByValue))
            {
                data = ODataOrderByOperator.Apply(data, properties.Value, orderByValue);
            }

            IQueryable<object> projected;

            if (query.HasParam("$select", out var selectValue) && !selectValue.Equals("*"))
            {
                projected = ODataSelectOperator.Apply(data, properties.Value, selectValue);
                hasSelect = true;
            }
            else
            {
                projected = data.Cast<object>();
            }

            if (query.HasIntParam("$skip", out int skip))
            {
                projected = projected.Skip(skip);
            }

            if (query.HasIntParam("$top", out int top))
            {
                projected = projected.Take(top);
            }

            var isAsync = projected.Provider is IAsyncQueryProvider;

            var result = query.ContainsKey("$count")
                ? new OkObjectResult(isAsync ? await projected.CountAsync() : projected.Count())
                : new OkObjectResult(isAsync ? await projected.ToListAsync() : projected.ToList());

            if (hasSelect)
            {
                result.Formatters.Add(SelectFormatter);
            }

            return result;
        }
    }
}
