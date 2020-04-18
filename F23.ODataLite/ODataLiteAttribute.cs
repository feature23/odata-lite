using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace F23.ODataLite
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ODataLiteAttribute : ResultFilterAttribute
    {   
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            context.Result = await ODataLiteActionResultHandler.HandleResult(
                context.Result, 
                context.HttpContext.Request.Query
            );

            await base.OnResultExecutionAsync(context, next);
        }
    }
}
