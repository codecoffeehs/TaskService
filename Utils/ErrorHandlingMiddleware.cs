using System.Net;
using System.Text.Json;
using TaskService.Exceptions;
using Microsoft.AspNetCore.Http;

namespace TaskService.Utils
{
    public class ErrorHandlingMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            HttpStatusCode statusCode = exception switch
            {
                BadRequestException => HttpStatusCode.BadRequest,
                ArgumentNullException => HttpStatusCode.BadRequest,
                ArgumentException => HttpStatusCode.BadRequest,
                InvalidOperationException => HttpStatusCode.BadRequest,

                UnauthorizedException => HttpStatusCode.Unauthorized,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,

                ForbiddenException => HttpStatusCode.Forbidden,

                NotFoundException => HttpStatusCode.NotFound,
                KeyNotFoundException => HttpStatusCode.NotFound,

                ConflictException => HttpStatusCode.Conflict,

                RequestTimeoutException => HttpStatusCode.RequestTimeout,
                TimeoutException => HttpStatusCode.RequestTimeout,

                _ => HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                success = false,
                statusCode = (int)statusCode,
                message = exception.Message
            };

            var payload = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return context.Response.WriteAsync(payload);
        }
    }
}