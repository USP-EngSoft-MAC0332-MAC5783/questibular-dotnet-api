namespace Portfolio.Api.Infrastructure;

public class SelfOnlyRequirement : IAuthorizationRequirement { }

public sealed class SelfOnlyHandler : AuthorizationHandler<SelfOnlyRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SelfOnlyRequirement requirement)
    {
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var routeId = context.Resource is HttpContext httpContext 
            ? httpContext.Request.RouteValues["id"]?.ToString() 
            : null;

        if (userId != null && userId == routeId)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}