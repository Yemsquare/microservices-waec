using Microsoft.AspNetCore.Mvc;

namespace AuthService.Common;

public static class ResultExtensions
{
    public static IActionResult ToProblem<T>(this ControllerBase c, Result<T> r) => r.Error switch
    {
        ErrorType.Validation when r.Errors is not null =>
            c.ValidationProblem(new ValidationProblemDetails(r.Errors)),
        ErrorType.Validation => c.BadRequest(new { message = r.Message }),
        ErrorType.Unauthorized => c.Unauthorized(),
        ErrorType.NotFound => c.NotFound(),
        _ => c.Problem()
    };
}

