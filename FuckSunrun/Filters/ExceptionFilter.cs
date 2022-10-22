using System;
using FuckSunrun.Common.DI;
using FuckSunrun.Common.R;
using FuckSunrun.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FuckSunrun.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var logger = DI.Services.CreateScope().ServiceProvider.GetRequiredService<ILogger<ExceptionFilter>>();

            if (context.ExceptionHandled)
            {
                return;
            }

            switch (context.Exception)
            {
                case BusinessException ex:
                    context.Result = GenerateErrorResult(context.HttpContext, ex.Message, ex.Code);
                    break;
                default:
                    context.Result = GenerateErrorResult(context.HttpContext, "未知的系统错误", 500);
                    logger.LogError(context.Exception,"错误过滤器监测到错误");
                    break;
            }

            context.ExceptionHandled = true;
        }

        private IActionResult GenerateErrorResult(HttpContext context,string message,int code)
        {
            if (context.Request.Headers["X-Requested-With"].FirstOrDefault() == "XMLHttpRequest")
            {
                return R.Json(message,status:code);
            }
            else
            {

                var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(),new ModelStateDictionary());

                viewData["code"] = code;
                viewData["message"] = message;

                return new ViewResult() { ViewName = "~/Views/Shared/Error.cshtml",ViewData=viewData,StatusCode = code};
            }
        }
    }


}

