using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BackendTemplate.Application.Features.Users;
using BackendTemplate.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BackendTemplate.Tests;

public sealed class UserApiTests : IAsyncLifetime
{
    private const string Password = "correct horse battery staple";
    private readonly ApiFactory _factory = new();
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task<UserResponse> RegisterAsync(string email = "alice@example.com")
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { FirstName = "Alice", LastName = "Example", Email = email, Password });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(user);
        Assert.Equal($"/api/users/{user.Id}", response.Headers.Location?.OriginalString);
        return user;
    }

    private async Task<string> LoginAsync(string email = "alice@example.com")
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        Assert.NotNull(auth);
        Assert.Equal("Bearer", auth.TokenType);
        Assert.True(auth.ExpiresAt > DateTimeOffset.UtcNow);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return auth.AccessToken;
    }

    [Fact]
    public async Task RegistrationPersistsNormalizedEmailAndPublishesEvent()
    {
        var user = await RegisterAsync(" Alice@Example.COM ");
        Assert.Equal("alice@example.com", user.Email);
        Assert.NotEqual(Guid.Empty, user.Id);
        await _factory.InDatabaseAsync(async db =>
        {
            var saved = await db.Users.SingleAsync();
            Assert.Equal(user.Id, saved.Id);
            Assert.NotEqual(Password, saved.PasswordHash);
            Assert.Empty(saved.DomainEvents);
            Assert.False(saved.IsSuperAdmin);
            Assert.Empty(saved.Roles);
            Assert.Empty(saved.Permissions);
        });
        Assert.Contains(_factory.Logs.Messages, message => message.Contains($"User {user.Id} registered"));
    }

    [Fact]
    public async Task ValidationReturnsFieldErrorsAndDoesNotPersistUser()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { FirstName = "", LastName = "", Email = "bad-email", Password = "short" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("Password", out _));
        Assert.True(problem.TryGetProperty("traceId", out _));
        await _factory.InDatabaseAsync(async db => Assert.False(await db.Users.AnyAsync()));
    }

    [Fact]
    public async Task DuplicateEmailIsCaseInsensitiveAndReturnsConflict()
    {
        await RegisterAsync();
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { FirstName = "Other", LastName = "Person", Email = "ALICE@EXAMPLE.COM", Password });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await _factory.InDatabaseAsync(async db => Assert.Equal(1, await db.Users.CountAsync()));
    }

    [Fact]
    public async Task ConcurrentRegistrationProducesOneAccount()
    {
        var responses = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ =>
            _client.PostAsJsonAsync("/api/auth/register",
                new { FirstName = "Alice", LastName = "Example", Email = "alice@example.com", Password })));
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Equal(3, responses.Count(response => response.StatusCode == HttpStatusCode.Conflict));
        await _factory.InDatabaseAsync(async db => Assert.Equal(1, await db.Users.CountAsync()));
    }

    [Fact]
    public async Task AuthenticatedUserCanReadOwnProfileWithoutExposingCredentials()
    {
        var user = await RegisterAsync();
        var token = await LoginAsync("ALICE@EXAMPLE.COM");
        foreach (var path in new[] { "/api/users/me", $"/api/users/{user.Id}" })
        {
            var response = await _client.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("permissions", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(token, body);
            Assert.Equal(user.Id, JsonSerializer.Deserialize<UserResponse>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web))!.Id);
        }
        Assert.DoesNotContain(_factory.Logs.Messages, message => message.Contains(Password) || message.Contains(token));
        await _factory.InDatabaseAsync(async db => Assert.NotNull((await db.Users.SingleAsync()).LastLoginAt));
    }

    [Fact]
    public async Task AnonymousUserReceivesUnauthorized()
    {
        var response = await _client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("Bearer", response.Headers.WwwAuthenticate.ToString());
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task OtherProfilesRequirePermissionAndRevocationTakesEffectImmediately()
    {
        var alice = await RegisterAsync();
        var bob = await RegisterAsync("bob@example.com");
        await LoginAsync();
        Assert.Equal(HttpStatusCode.Forbidden, (await _client.GetAsync($"/api/users/{bob.Id}")).StatusCode);

        await _factory.InDatabaseAsync(async db =>
        {
            var user = await db.Users.FindAsync(alice.Id);
            user!.AddPermission(new Permission("users:read", "Read user accounts"));
            await db.SaveChangesAsync();
        });
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/api/users/{bob.Id}")).StatusCode);

        await _factory.InDatabaseAsync(async db =>
        {
            var user = await db.Users.Include(x => x.Permissions).SingleAsync(x => x.Id == alice.Id);
            user.Permissions.Clear();
            await db.SaveChangesAsync();
        });
        Assert.Equal(HttpStatusCode.Forbidden, (await _client.GetAsync($"/api/users/{bob.Id}")).StatusCode);
    }

    [Fact]
    public async Task RolePermissionsAllowReadsAndMissingUsersReturnNotFound()
    {
        var alice = await RegisterAsync();
        await _factory.InDatabaseAsync(async db =>
        {
            var role = new Role("Reader", null);
            role.AddPermission(new Permission("users:read", null));
            (await db.Users.FindAsync(alice.Id))!.AddRole(role);
            await db.SaveChangesAsync();
        });
        await LoginAsync();
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/users/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task FailedLoginsLockAccountAndLockoutExpires()
    {
        await RegisterAsync();
        for (var i = 0; i < 5; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login",
                new { Email = "alice@example.com", Password = "incorrect password" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        var locked = await _client.PostAsJsonAsync("/api/auth/login",
            new { Email = "alice@example.com", Password });
        Assert.Equal(HttpStatusCode.Unauthorized, locked.StatusCode);
        await _factory.InDatabaseAsync(async db =>
        {
            var user = await db.Users.SingleAsync();
            Assert.Equal(5, user.LoginFailedCount);
            Assert.True(user.IsLockedOut(_factory.Clock.GetUtcNow()));
        });

        _factory.Clock.Advance(TimeSpan.FromMinutes(16));
        await LoginAsync();
        await _factory.InDatabaseAsync(async db =>
        {
            var user = await db.Users.SingleAsync();
            Assert.Equal(0, user.LoginFailedCount);
            Assert.Null(user.LockoutEnd);
        });
    }

    [Fact]
    public async Task AccountLockoutAlsoInvalidatesAnExistingToken()
    {
        await RegisterAsync();
        await LoginAsync();
        await _factory.InDatabaseAsync(async db =>
        {
            var user = await db.Users.SingleAsync();
            for (var i = 0; i < 5; i++) user.RecordFailedLogin(_factory.Clock.GetUtcNow());
            await db.SaveChangesAsync();
        });
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/users/me")).StatusCode);
    }

    [Theory]
    [InlineData("expired")]
    [InlineData("issuer")]
    [InlineData("audience")]
    [InlineData("signature")]
    [InlineData("algorithm")]
    public async Task InvalidTokensAreRejected(string invalidPart)
    {
        var user = await RegisterAsync();
        var now = DateTime.UtcNow;
        var key = invalidPart == "signature" ? ApiFactory.SigningKey + "-wrong" : ApiFactory.SigningKey;
        var token = new JwtSecurityToken(
            issuer: invalidPart == "issuer" ? "wrong" : "BackendTemplate",
            audience: invalidPart == "audience" ? "wrong" : "BackendTemplate.Client",
            claims: [new Claim("sub", user.Id.ToString())],
            notBefore: now.AddHours(-2),
            expires: invalidPart == "expired" ? now.AddHours(-1) : now.AddHours(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                invalidPart == "algorithm" ? SecurityAlgorithms.HmacSha384 : SecurityAlgorithms.HmacSha256));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            new JwtSecurityTokenHandler().WriteToken(token));
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/users/me")).StatusCode);
    }

    [Fact]
    public async Task HealthChecksAndOpenApiAreAvailable()
    {
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/health/ready")).StatusCode);
        var response = await _client.GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(document.GetProperty("paths").TryGetProperty("/api/auth/register", out _));
        Assert.True(document.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _));
        Assert.True(document.GetProperty("paths").GetProperty("/api/users/me").GetProperty("get")
            .TryGetProperty("security", out _));
        Assert.False(document.GetProperty("paths").GetProperty("/api/auth/login").GetProperty("post")
            .TryGetProperty("security", out _));
    }

    [Fact]
    public async Task AuthenticationEndpointsAreRateLimited()
    {
        await using var factory = new ApiFactory(permitLimit: 2);
        using var client = factory.CreateClient();
        for (var i = 0; i < 2; i++)
            Assert.Equal(HttpStatusCode.BadRequest,
                (await client.PostAsJsonAsync("/api/auth/login", new { Email = "", Password = "" })).StatusCode);
        var response = await client.PostAsJsonAsync("/api/auth/login", new { Email = "", Password = "" });
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.NotNull(response.Headers.RetryAfter);
    }
}
