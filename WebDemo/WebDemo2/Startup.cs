using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebDemo2.Policy;
using Microsoft.AspNetCore.Authorization;
using WebDemo2.Middleware;

namespace WebDemo2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// �����ֹ���ȡע��
        /// </summary>
        private static IServiceProvider serviceProvider;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddControllers();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AtLeast21", policy =>
                    policy.Requirements.Add(new MinimumAgeRequirement(21)));
            });

            //���jwt��֤��
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateLifetime = true,//�Ƿ���֤ʧЧʱ��
                        ClockSkew = TimeSpan.FromSeconds(30),

                        ValidateAudience = true,//�Ƿ���֤Audience
                        ValidAudience = this.Configuration["JwtBearer:Audience"],//Audience
                        //AudienceValidator = (m, n, z) =>
                        //{
                        //    return m != null && m.FirstOrDefault().Equals(this.Configuration["Audience"]);
                        //},

                        ValidateIssuer = true,//�Ƿ���֤Issuer
                        ValidIssuer = this.Configuration["JwtBearer:Issuer"],//Issuer���������ǰ��ǩ��jwt������һ��

                        ValidateIssuerSigningKey = true,//�Ƿ���֤SecurityKey
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["JwtBearer:SecurityKey"]))//�õ�SecurityKey
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            //Token expired
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        }
                    };
                });


            services.AddSingleton(Configuration);
            services.AddSingleton<Service.CaptchaHelper>();
            services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.KnownProxies.Add(IPAddress.Parse(Configuration["Proxy"]));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            serviceProvider = app.ApplicationServices;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<AdminSafeListMiddleware>(Configuration["AdminSafeList"]);

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public static IConfiguration GetConfiguration()
        {
            return serviceProvider.GetService<IConfiguration>();
        }
    }
}
