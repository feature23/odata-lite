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
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using F23.Hateoas;

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
            if (!(context.Result is OkObjectResult ok))
            {
                await base.OnResultExecutionAsync(context, next);
                return;
            }

            if (!ok.Value.IsQueryableOrEnumerable(out var rawData) 
                || (ok.Value is HypermediaResponse hypermedia && !hypermedia.Content.IsQueryableOrEnumerable(out rawData)))
            {
                throw new InvalidOperationException("Data (or HypermediaResponse.Content) must be IQueryable<T> or IEnumerable<T> to use ODataLite. Pass a queryable or enumerable, or remove this attribute.");
            }

            var itemType = rawData.GetType().GetGenericArguments().First();

            var applyMethod = typeof(ODataLiteAttribute)
                .GetMethod(nameof(ApplyODataAsync), BindingFlags.Static | BindingFlags.NonPublic);
            
            Debug.Assert(applyMethod != null, nameof(applyMethod) + " != null");

            applyMethod = applyMethod.MakeGenericMethod(itemType);

            if (applyMethod.Invoke(null, new object[] { context, rawData }) is Task<OkObjectResult> result)
            {
                context.Result = await result;
            }

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
