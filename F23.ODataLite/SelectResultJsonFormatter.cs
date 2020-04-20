using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Buffers;

namespace F23.ODataLite
{
    internal class SelectResultJsonFormatter : NewtonsoftJsonOutputFormatter
    {
        public static readonly SelectResultJsonFormatter Instance = new SelectResultJsonFormatter();

        private SelectResultJsonFormatter() 
            : base(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy
                    {
                        ProcessDictionaryKeys = true
                    }
                }
            }, ArrayPool<char>.Shared, new MvcOptions())
        {
        }
    }
}
