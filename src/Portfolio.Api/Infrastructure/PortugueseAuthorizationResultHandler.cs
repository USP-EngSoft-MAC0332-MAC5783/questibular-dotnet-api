namespace Portfolio.Api.Infrastructure;

using Microsoft.AspNetCore.Authorization.Policy;
using System.Text.Json;

public sealed class PortugueseAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
    private static readonly JsonSerializerOptions ProblemJsonOptions = new(JsonSerializerDefaults.Web);

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded)
        {
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
            return;
        }

        var status = authorizeResult.Challenged
            ? StatusCodes.Status401Unauthorized
            : StatusCodes.Status403Forbidden;

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            new ProblemDetails
            {
                Type = $"https://httpstatuses.io/{status}",
                Title = authorizeResult.Challenged ? "Autenticação necessária." : "Acesso negado.",
                Status = status,
                Detail = authorizeResult.Challenged
                    ? "É necessário autenticar-se para acessar este recurso."
                    : "Você não tem permissão para acessar este recurso.",
                Instance = context.Request.Path
            },
            options: ProblemJsonOptions,
            cancellationToken: context.RequestAborted);
    }
}
