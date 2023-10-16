using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TT.BaseProject.Application.Exceptions;

namespace TT.BaseProject.HostBase.Filter
{
    public class CustomExceptionFilter : IActionFilter, IOrderedFilter
    {
        public int Order => int.MaxValue - 10;
        private readonly ILogger _log;
        private readonly IConfiguration _configuration;

        public CustomExceptionFilter(ILogger<CustomExceptionFilter> log, IConfiguration configuration)
        {
            _log = log;
            _configuration = configuration;
        }

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {

            if (context.Exception is BusinessException businessException)
            {
                context.Result = new ObjectResult(businessException)
                {
                    StatusCode = 550,
                    Value = businessException.GetClientReturn()
                };

                context.ExceptionHandled = true;
            }
            else if (context.Exception != null)
            {
                _log.LogError(context.Exception, context.Exception.Message);
                var msg = "Exception";
                var showExceptionResponse = false;
#if DEBUG
                showExceptionResponse = true;
#endif
                if (showExceptionResponse)
                {
                    msg = context.Exception.Message;
                }
                context.Result = new ObjectResult(context.Exception)
                {
                    StatusCode = 500,
                    Value = msg
                };

                context.ExceptionHandled = true;
            }
        }
    }
}
