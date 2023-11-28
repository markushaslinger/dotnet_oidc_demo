using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace KeyCloakDemo.Client;

public sealed class PersistentAuthenticationStateProvider(PersistentComponentState persistentState) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        Console.WriteLine(JsonSerializer.Serialize(persistentState));
        if (!persistentState.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) || userInfo is null)
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));;
        }

        Claim[] claims = [
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId),
            new Claim(ClaimTypes.Name, userInfo.Email),
            new Claim(ClaimTypes.Email, userInfo.Email)];

        return Task.FromResult(
                               new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims,
                                                                            authenticationType: nameof(PersistentAuthenticationStateProvider)))));
    }
}

public class UserInfo
{
    public required string UserId { get; set; }
    public required string Email { get; set; }
}
