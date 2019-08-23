using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Services
{
    public sealed class NodeAuthenticationStateProvider : AuthenticationStateProvider
    {
        public NodeAuthenticationStateProvider()
        {
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new GenericIdentity(Environment.UserDomainName + "\\" + Environment.UserName, "Anonymous");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }
    }
}