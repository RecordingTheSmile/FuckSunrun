using System;
using System.Net;
using FuckSunrun.Common.R;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FuckSunrun.Filters
{
    public class ActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if(context.Result is StatusCodeResult result)
            {
                context.Result = R.Json(GetStatusCodePharse(result.StatusCode),status:result.StatusCode);
            }
            else if(context.Result is ObjectResult objectResult)
            {
                context.Result = R.Json(
                    objectResult.Value is string ?
                    objectResult.Value as string ?? "OK"
                    :
                    GetStatusCodePharse(objectResult.StatusCode ?? 200)
                    , objectResult.Value is not string ?
                    objectResult.Value
                    :
                    null,
                    objectResult.StatusCode ?? 200);
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = R.Json(context.ModelState.FirstOrDefault(x=>(x.Value?.Errors?.Count ?? 0) != 0).Value?.Errors.FirstOrDefault()?.ErrorMessage ?? "您的请求参数不正确");
            }
        }

        private string GetStatusCodePharse(int statusCode)
        {
            return Enum.GetName(typeof(HttpStatusCode), statusCode) ?? "UnknownError";
        }
    }
}

