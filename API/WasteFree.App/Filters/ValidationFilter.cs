using FluentValidation;

namespace WasteFree.App.Filters;

public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var validator = ctx.HttpContext.RequestServices.GetService<IValidator<T>>();
        var arg = ctx.GetArgument<T>(0);

        if (validator is not null)
        {
            var result = await validator.ValidateAsync(arg);
            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                return Results.UnprocessableEntity(errors); // 422
            }
        }
        return await next(ctx);
    }
}