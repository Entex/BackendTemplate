using System.Security.Claims;
using BackendTemplate.Application.Features.Users;
using BackendTemplate.Application.Features.Users.Commands;
using BackendTemplate.Application.Features.Users.Queries;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BackendTemplate.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/api/auth").WithTags("Authentication")
            .RequireRateLimiting("authentication");
        auth.MapPost("/register", async Task<Created<UserResponse>> (
            CreateUserCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var user = await sender.Send(command, cancellationToken);
            return TypedResults.Created($"/api/users/{user.Id}", user);
        }).WithName("RegisterUser")
            .WithSummary("Register an account without administrative privileges.")
            .ProducesValidationProblem().ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        auth.MapPost("/login", async Task<Ok<AuthenticationResponse>> (
            AuthenticateCommand command, ISender sender, CancellationToken cancellationToken) =>
            TypedResults.Ok(await sender.Send(command, cancellationToken)))
            .WithName("Login").WithSummary("Exchange credentials for a short-lived bearer token.")
            .ProducesValidationProblem().ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        var users = endpoints.MapGroup("/api/users").WithTags("Users").RequireAuthorization();
        users.MapGet("/me", async Task<Ok<UserResponse>> (
            ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken) =>
            TypedResults.Ok(await sender.Send(
                new GetUserByIdQuery(Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!)), cancellationToken)))
            .WithName("GetCurrentUser").WithSummary("Read your own account.")
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        users.MapGet("/{id:guid}", async Task<Ok<UserResponse>> (
            Guid id, ISender sender, CancellationToken cancellationToken) =>
            TypedResults.Ok(await sender.Send(new GetUserByIdQuery(id), cancellationToken)))
            .WithName("GetUserById").WithSummary("Read your own account, or another account with users:read permission.")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }
}
