using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebDemo2.Middleware
{
    public static class BasicAuthenticationScheme
    {
        public const string DefaultScheme = "Basic";
    }

    public class BasicAuthenticationOption : AuthenticationSchemeOptions
    {
        public const string SettingPath = "InvoiceSetting";
    
        public string Realm { get; set; }
        public string AppId { get; set; }
        public string AppSecret { get; set; }
    }

    public class BasicAuthorizationHandler : AuthenticationHandler<BasicAuthenticationOption>
    {
        public BasicAuthorizationHandler(IOptionsMonitor<BasicAuthenticationOption> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        
        }

        /// <summary>
        /// 认证逻辑
        /// </summary>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");
        
            string username, password;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
                username = credentials[0];
                password = credentials[1];
                var isValidUser = IsAuthorized(username, password);
                if (isValidUser == false) return AuthenticateResult.Fail("Invalid username or password");
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
        
            var claims = new[]
                { new Claim(ClaimTypes.NameIdentifier, username), new Claim(ClaimTypes.Name, password), };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }

        /// <summary>
        /// 质询
        /// </summary>
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{Options.Realm}\"";
            await base.HandleChallengeAsync(properties);
        }

        /// <summary>
        /// 认证失败
        /// </summary>
        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            await base.HandleForbiddenAsync(properties);
        }

        private bool IsAuthorized(string username, string password)
        {
            return username.Equals(Options.AppId, StringComparison.InvariantCultureIgnoreCase) &&
                   password.Equals(Options.AppSecret);
        }
    }

// HTTP基本认证Middleware
    public static class BasicAuthentication
    {
        public static void UseBasicAuthentication(this IApplicationBuilder app)
        {
            app.UseWhen(x => x.Request.Path.StartsWithSegments(new PathString("/api/v1/Invoice/InvoiceCallback")),
                appBuilder =>
                {
                    appBuilder.UseMiddleware<BasicAuthenticationMiddleware>();
                });
        }
    }

    public class BasicAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public BasicAuthenticationMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<BasicAuthenticationMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext, IAuthenticationService authenticationService)
        {
            var authenticated =
                await authenticationService.AuthenticateAsync(httpContext, BasicAuthenticationScheme.DefaultScheme);
        
            _logger.LogInformation("BasicAuthenticationMiddleware Access Status：" + authenticated.Succeeded);
        
            if (!authenticated.Succeeded)
            {
                await authenticationService.ChallengeAsync(httpContext, BasicAuthenticationScheme.DefaultScheme,
                    new AuthenticationProperties());
                return;
            }

            await _next(httpContext);
        }
    }
}