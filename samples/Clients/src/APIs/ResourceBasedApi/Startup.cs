﻿using Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.AccessTokenValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;

namespace ResourceBasedApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddCors();
            services.AddDistributedMemoryCache();

            services.AddAuthentication("token")

                // dispatches to appropriate handler
                .AddDynamicAuthenticationHandler("token", options =>
                {
                    options.DefaultScheme = DynamicAuthenticationHandlerDefaults.JwtBearerDefaultScheme;
                    options.SchemeSelector = JwtAndIntrospectionSelector.Func;
                })
                
                // JWT token
                .AddJwtBearer(DynamicAuthenticationHandlerDefaults.JwtBearerDefaultScheme, options =>
                {
                    options.Authority = Constants.Authority;
                    options.Audience = "resource1";

                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
                })

                // reference tokens
                .AddOAuth2Introspection(DynamicAuthenticationHandlerDefaults.IntrospectionDefaultScheme, options =>
                {
                    options.Authority = Constants.Authority;

                    options.ClientId = "resource1";
                    options.ClientSecret = "secret";
                });

            services.AddScopeTransformation();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCors(policy =>
            {
                policy.WithOrigins(
                    "https://localhost:44300");

                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.WithExposedHeaders("WWW-Authenticate");
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization();
            });
        }
    }
}