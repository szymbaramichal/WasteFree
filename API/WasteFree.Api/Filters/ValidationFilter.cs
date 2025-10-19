using FluentValidation;

namespace WasteFree.Api.Filters;

public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var validator = ctx.HttpContext.RequestServices.GetService<IValidator<T>>();

        T? arg = default;

        for (var i = 0; i < ctx.Arguments.Count; i++)
        {
            var raw = ctx.Arguments[i];

            if (raw is T casted)
            {
                arg = casted;
                break;
            }

            if (raw is null)
            {
                try
                {
                    arg = ctx.GetArgument<T>(i);
                    break;
                }
                catch (InvalidCastException)
                {
                    //Ignore
                }
            }
        }

        if (validator is not null)
        {
            if (arg is null)
            {
                var missingBody = new Dictionary<string, string[]>
                {
                    { string.Empty, new[] { "Request body is required." } }
                };
                return Results.UnprocessableEntity(missingBody);
            }

            var result = await validator.ValidateAsync(arg);
            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                return Results.UnprocessableEntity(errors);
            }
        }
        return await next(ctx);
    }
}