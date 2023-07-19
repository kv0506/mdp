using MDP.API.Model;
using MDP.Exceptions;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text.Json;

namespace MDP.API.Middleware;

public class AppExceptionHandlerMiddleware
{
    private readonly ILogger<AppExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly RequestDelegate _next;

    public static string InternalServerErrorMessage { get; set; } =
        "We're experiencing some difficulties at this time,. Please try again later.";

    public AppExceptionHandlerMiddleware(RequestDelegate next, ILogger<AppExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        this._next = next;
        this._logger = logger;
        this._environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ExceptionDispatchInfo edi = null;

        try
        {
            await _next(context);
        }
        catch (System.Exception exception)
        {
            edi = ExceptionDispatchInfo.Capture(exception);
        }

        if (edi != null)
        {
            await HandleExceptionAsync(context, edi);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, ExceptionDispatchInfo exception)
    {
        try
        {
            if (context.Response.HasStarted)
                return;

            // log the error
            _logger.LogError(exception.SourceException, exception.SourceException.Message);

            context.Response.ContentType = "application/json";

            var status = (int)HttpStatusCode.InternalServerError;
            var response = new ApiResponse
            {
                Errors = new List<Error>
                {
                    new Error(ErrorCode.BadRequest, _environment.IsDevelopment()
                        ? exception.SourceException.ToString()
                        : InternalServerErrorMessage)
                }
            };

            if (exception.SourceException is MDPException mdpException)
            {
                status = mdpException.ErrorCode.ToHttpStatusCode();
                response.Errors = new List<Error>
                {
                    new Error(mdpException.ErrorCode, mdpException.Message)
                };
            }

            context.Response.StatusCode = status;
            await context.Response.WriteAsync(JsonSerializer.Serialize(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            await context.Response.Body.FlushAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}