
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate requestDelegate;
    private readonly ILogger<ExceptionHandlingMiddleware> logger;
    public ExceptionHandlingMiddleware(RequestDelegate _next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        this.requestDelegate = _next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await requestDelegate(context);

        }
        catch (Exception ex)
        {
             logger.LogError(ex, "An unexpected error occurred.");
            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new {message = "An unexpected error occurred."});
            }else
            {
                throw;
            }  
        }
    }

}