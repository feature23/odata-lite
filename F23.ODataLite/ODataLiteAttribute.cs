﻿using F23.ODataLite.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Buffers;
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
        private static readonly NewtonsoftJsonOutputFormatter SelectFormatter = new NewtonsoftJsonOutputFormatter(new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true
                }
            }
        }, ArrayPool<char>.Shared, new MvcOptions());

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (!(context.Result is ObjectResult ok))
            {
                await base.OnResultExecutionAsync(context, next);
                return;
            }

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

            var applyMethod = typeof(ODataLiteAttribute)
                .GetMethod(nameof(ApplyODataAsync), BindingFlags.Static | BindingFlags.NonPublic);
            
            Debug.Assert(applyMethod != null, nameof(applyMethod) + " != null");

            applyMethod = applyMethod.MakeGenericMethod(itemType);

            if (applyMethod.Invoke(null, new object[] { context, rawData, ok.Value as HypermediaResponse, isQueryable }) is Task<ObjectResult> result)
            {
                context.Result = await result;
            }

            await base.OnResultExecutionAsync(context, next);
        }
        
        private static async Task<ObjectResult> ApplyODataAsync<T>(ActionContext context, IQueryable rawData, HypermediaResponse hypermediaResponse, bool isQueryable)
        {
            var data = (IQueryable<T>)rawData;

            var properties = new Lazy<PropertyInfo[]>(() => typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance));

            var query = context.HttpContext.Request.Query;
            bool hasSelect = false;

            if (query.HasParam("$filter", out var filterValue))
            {
                data = ODataFilterOperator.Apply(data, properties.Value, filterValue, isQueryable);
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

            bool isAsync = projected.Provider is IAsyncQueryProvider;

            var result = query.ContainsKey("$count")
                ? CreateResult(isAsync ? await projected.CountAsync() : projected.Count(), hypermediaResponse)
                : CreateResult(isAsync ? await projected.ToListAsync() : projected.ToList(), hypermediaResponse);

            if (hasSelect)
            {
                result.Formatters.Add(SelectFormatter);
            }

            return result;
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
