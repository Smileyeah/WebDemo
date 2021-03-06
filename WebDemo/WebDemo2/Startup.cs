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
using WebDemo2.Extensions;
using System.Security.Claims;
using WebDemo2.Filter;
using Microsoft.AspNetCore.Identity;
using WebDemo.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

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
        /// 用于手工获取注入
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add EF services to the services container.
            services.AddDbContextPool<ApplicationDbContext>(d => d.UseSqlite($"Filename=./Database.db"), 512);

            IdentityBuilder builder = services.AddIdentityCore<IdentityUser>(options => { });
            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), builder.Services);
            builder.AddRoleManager<RoleManager<IdentityRole>>();
            builder.AddSignInManager<SignInManager<IdentityUser>>();
            builder.AddEntityFrameworkStores<ApplicationDbContext>();
            builder.AddDefaultTokenProviders();

            var mvcBuilders = services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                options.Filters.Add(typeof(WebGlobalExecptionFilter));
                options.Filters.Add(typeof(CustomFilterAttribute));
                options.Filters.Add(typeof(HeaderCheckFilter));
            });

            services.AddMemoryCache();
            services.AddControllers();
            services.AddHttpContextAccessor();

            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("AtLeast21", policy =>
            //        policy.Requirements.Add(new MinimumAgeRequirement(21)));
            //});

            //【授权】
            services.AddAuthorization(options =>
            {
                //权限要求参数
                var permissionRequirement = new PermissionRequirement(
                    ClaimTypes.Name,// 基于用户名的授权
                    expiration: TimeSpan.FromMinutes(120),// 接口的过期时间
                    "/api/nopermission"// 拒绝授权的跳转地址
                );

                // 添加策略鉴权模式
                options.AddPolicy("Permission", policy => policy.Requirements.Add(permissionRequirement));

                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme,
                    new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
            });

            //添加jwt验证：
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateLifetime = true,//是否验证失效时间
                        ClockSkew = TimeSpan.FromSeconds(30),

                        ValidateAudience = true,//是否验证Audience
                        ValidAudience = this.Configuration["JwtBearer:Audience"],//Audience
                        //AudienceValidator = (m, n, z) =>
                        //{
                        //    return m != null && m.FirstOrDefault().Equals(this.Configuration["Audience"]);
                        //},

                        ValidateIssuer = true,//是否验证Issuer
                        ValidIssuer = this.Configuration["JwtBearer:Issuer"],//Issuer，这两项和前面签发jwt的设置一致

                        ValidateIssuerSigningKey = true,//是否验证SecurityKey
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["JwtBearer:SecurityKey"]))//拿到SecurityKey
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
            //services.AddTransient<IAuthorizationHandler, MinimumAgeHandler>();
            services.AddTransient<IAuthorizationHandler, PermissionHandler>();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.KnownProxies.Add(IPAddress.Parse(Configuration["Proxy"]));
            });

            // 后台定时服务
            //services.AddHostedService<Service.BackgroundService.TimedHostedService>();

            // RabbitMQ消息服务
            services.AddRabbit(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ServiceProvider = app.ApplicationServices;

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
            return ServiceProvider.GetService<IConfiguration>();
        }
    }
}
