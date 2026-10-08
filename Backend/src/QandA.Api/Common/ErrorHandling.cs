using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace QandA.Api.Common;

/// <summary>Turns <see cref="AppException"/> into RFC 7807 problem details with a <c>code</c> extension.</summary>
public class AppExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not AppException app)
            return false;

        context.Response.StatusCode = app.Status;
        var problem = new ProblemDetails { Status = app.Status, Title = app.Message };
        problem.Extensions["code"] = app.Code;
        if (app.Field is not null)
            problem.Extensions["errors"] = new Dictionary<string, string[]> { [app.Field] = [app.Code] };

        return await problemDetails.TryWriteAsync(new() { HttpContext = context, ProblemDetails = problem, Exception = exception });
    }
}

public static class ValidationResponse
{
    /// <summary>Model-binding / data-annotation failures, shaped like <see cref="AppException"/> errors.</summary>
    public static IActionResult Create(ActionContext context)
    {
        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more fields are invalid.",
        };
        problem.Extensions["code"] = "validation";
        return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
    }
}
