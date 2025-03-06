using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cantina.Infrastructure.Persistence.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = exception switch
            {
                KeyNotFoundException => new { StatusCode = (int)HttpStatusCode.NotFound, Message = "Resource not found" },
                ArgumentException => new { StatusCode = (int)HttpStatusCode.BadRequest, exception.Message },
                _ => new { StatusCode = (int)HttpStatusCode.InternalServerError, Message = "An unexpected error occurred." }
            };

            response.StatusCode = errorResponse.StatusCode;
            return response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}
