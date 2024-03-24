using System;
using System.Net;
using Grpc.Core;

namespace Ozon.Route256.Practice.GatewayService.Middleware
{
    public sealed class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(RequestDelegate next, 
            ILogger<ExceptionHandler> logger)
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var errorMessage = "An error occurred on the server.";
            _logger.LogError(ex, errorMessage);

            ExceptionResponse response = ex switch
            {
                RpcException => CreateResponse((RpcException)ex),
                _ => CreateResponse(ex, errorMessage)
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)response.StatusCode;
            await context.Response.WriteAsJsonAsync(response);
        }

        private ExceptionResponse CreateResponse(RpcException ex) =>
            ex.Status.StatusCode switch
            {
                StatusCode.NotFound => new(HttpStatusCode.NotFound, ex.Status.Detail),
                StatusCode.FailedPrecondition => new(HttpStatusCode.BadRequest, ex.Status.Detail),
                StatusCode.Cancelled => new(HttpStatusCode.RequestTimeout, ex.Status.Detail),
                StatusCode.Unknown => new(HttpStatusCode.InternalServerError, ex.Status.Detail),
                StatusCode.InvalidArgument => new(HttpStatusCode.InternalServerError, ex.Status.Detail),
                _ => new(HttpStatusCode.InternalServerError, ex.Status.Detail)
            };

        private ExceptionResponse CreateResponse(Exception ex, string errorMessage) =>
            ex switch
            {
                _ => new(HttpStatusCode.InternalServerError, errorMessage)
            };

        private record ExceptionResponse(HttpStatusCode StatusCode, string Description);
    }
}
