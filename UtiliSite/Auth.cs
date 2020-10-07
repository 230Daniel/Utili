using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace UtiliSite
{
    public class Auth
    {
        public static AuthUserDetails GetAuthUser(HttpContext httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                httpContext.ChallengeAsync("Discord", new AuthenticationProperties {RedirectUri = "/Dashboard"});
                return new AuthUserDetails(false);
            }

            AuthUserDetails userDetails = new AuthUserDetails(
                true,
                httpContext.User.FindFirst(x => x.Type.Contains("identity/claims/nameidentifier")).Value,
                httpContext.User.Identity.Name,
                httpContext.User.FindFirst(x => x.Type.Contains("discord:user:discriminator")).Value,
                httpContext.User.FindFirst(x => x.Type.Contains("discord:avatar:url")).Value);

            return userDetails;
        }
    }

    public class AuthUserDetails
    {
        public bool Authenticated { get; }
        public ulong Id { get; }
        public string Username { get; }
        public int Discriminator { get; }
        public string AvatarUrl { get; }

        public AuthUserDetails(bool authenticated, string id, string username, string discriminator, string avatarUrl)
        {
            Authenticated = authenticated;
            Id = ulong.Parse(id);
            Username = username;
            Discriminator = int.Parse(discriminator);
            AvatarUrl = avatarUrl;
        }
        
        public AuthUserDetails(bool authenticated)
        {
            Authenticated = authenticated;
        }
    }
}
