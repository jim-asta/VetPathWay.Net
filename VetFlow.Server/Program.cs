using MailerSend.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using VetFlow.Server.Services;
namespace VetFlow.Server
{
    public class Program        // Explicitly declare a Program class and Main method to allow a _logger helper method
    {
        // _logger helper method for request events
        static ILogger<Program> _logger(HttpContext context) => context.RequestServices.GetRequiredService<ILogger<Program>>();
        static ILogger<Program> _logger(Microsoft.AspNetCore.Authentication.BaseContext<JwtBearerOptions> context) => _logger(context.HttpContext);


        public static void Main(string[] args)
        {
#if DEBUG
            IdentityModelEventSource.ShowPII = true;
#endif

            var builder = WebApplication.CreateBuilder(args);
            IConfiguration _config = builder.Configuration;

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    policy.WithOrigins("https://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // needed if using cookies (optional)
                });
            });

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>      // Don't pass the empty (null) fields forward to the front-end in DTOs (maybe we _do_ want them?)
                 {
                     options.JsonSerializerOptions.DefaultIgnoreCondition =
                         System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                 });

            builder.Services.AddSingleton<GraphService>();
            builder.Services.AddOpenApi();
            builder.Services.AddHttpClient();
            builder.Services.Configure<MailerSendOptions>(_config.GetSection("MailerSend"));
            builder.Services.AddMailerSend();

            // JWT Bearer authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    //options.Authority = "https://login.microsoftonline.com/" + _config["AzureAd:TenantId"] + "/v2.0";
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        //ValidateIssuer = true,
                        //ValidIssuers = new[] {
                        //    "https://login.microsoftonline.com/" + _config["AzureAd:TenantId"] + "/v2.0",
                        //    "https://sts.windows.net/" + _config["AzureAd:TenantId"]+ "/"
                        //}
                        ValidateIssuer = false, // We'll validate manually
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidAudiences = new[]
                        {
                _config["Google:ClientId"],
                _config["AzureAd:ClientId"],
                "api://" + _config["AzureAd:ClientId"]
                        },

                        IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                        {
                            // This will automatically fetch keys from both Google and Microsoft
                            var googleKeys = new ConfigurationManager<OpenIdConnectConfiguration>(
                                "https://accounts.google.com/.well-known/openid-configuration",
                                new OpenIdConnectConfigurationRetriever()
                            ).GetConfigurationAsync().Result.SigningKeys;

                            var msftKeys = new ConfigurationManager<OpenIdConnectConfiguration>(
                                "https://login.microsoftonline.com/" + _config["AzureAd:TenantId"] + "/v2.0/.well-known/openid-configuration",
                                new OpenIdConnectConfigurationRetriever()
                            ).GetConfigurationAsync().Result.SigningKeys;

                            return googleKeys.Concat(msftKeys);
                        }
                    };

#if DEBUG           // We really don't need this in Release, good for diagnostic tho
                    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            _logger(context.HttpContext).LogError("Authentication failed: " + context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            _logger(context.HttpContext).LogInformation("Token validated successfully");

                            // Truncate if c.Value is too long
                            var claims = context.Principal?.Claims?.Select(c => c.Type + ": " + (c.Value?.Length > 100 ? c.Value[..100] + "..." : c.Value));
                            _logger(context.HttpContext).LogDebug("Claims: " + string.Join(" | ", claims ?? Array.Empty<string>()));

                            var issuer = context.Principal?.FindFirst("iss")?.Value;
                            var validIssuers = new[]
                            {
                    "https://accounts.google.com",
                    "accounts.google.com",
                    "https://login.microsoftonline.com/" + _config["AzureAd:TenantId"] + "/v2.0",
                    "https://sts.windows.net/" + _config["AzureAd:TenantId"] + "/"
                            };

                            if (!validIssuers.Contains(issuer))
                                context.Fail("Invalid issuer: " + issuer);

                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            _logger(context.HttpContext).LogInformation("Message received for: " + context.Request.Path);

                            var authHeader = context.Request.Headers["Authorization"].ToString();
                            if (string.IsNullOrEmpty(authHeader))
                                _logger(context.HttpContext).LogWarning("No Authorization header found");
                            else
                                _logger(context.HttpContext).LogInformation("Auth header: " + authHeader.Substring(0, Math.Min(50, authHeader.Length)) + "...");

                            return Task.CompletedTask;
                        }
                    };
#endif
                });

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Required with SameSite=None
            });


            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                context.Response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:;";

                _logger(context).LogInformation("Request to: " + context.Request.Path);
                _logger(context).LogInformation("Authorization Header: " + (string.IsNullOrEmpty(authHeader) ? "NONE" : authHeader.Substring(0, Math.Min(50, authHeader.Length))) + "...");
                await next();
            });

            app.UseDefaultFiles();
            app.MapStaticAssets();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseCors("FrontendPolicy");
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}