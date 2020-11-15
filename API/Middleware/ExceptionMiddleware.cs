using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using API.Errors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        readonly RequestDelegate _next ; 
        readonly ILogger<ExceptionMiddleware> _logger ;
        readonly IHostEnvironment _env ;
        
        public ExceptionMiddleware(RequestDelegate next, 
                                    ILogger<ExceptionMiddleware> logger, 
                                    IHostEnvironment env) 
        {
            _next = next ;
            _logger = logger ;
            _env = env ;    
        }

        public async Task InvokeAsync(HttpContext context) 
        {
            try 
            {
                await _next(context) ;
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, exp.Message) ;
                context.Response.ContentType = "application/json" ;
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError ;
                
                var response = _env.IsDevelopment() 
                ? new APIException(context.Response.StatusCode, exp.Message, exp.StackTrace?.ToString())
                : new APIException(context.Response.StatusCode, "Internal Server Error") ;

                var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase} ;

                var json = JsonSerializer.Serialize(response, options) ;

                await context.Response.WriteAsync(json) ;
            }
        }
    }
}