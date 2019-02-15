using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIdMvc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // 
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            //将认证服务添加到依赖注入容器中
            services.AddAuthentication(options =>
            {
                //使用cookie作为验证用户的主要方法
                options.DefaultScheme = "Cookies";
                //用户登录时使用odic方案
                options.DefaultChallengeScheme = "oidc";
            })
                //添加cookie的处理程序
                .AddCookie()
                //配置执行openid connection协议的处理程序
                //.AddOpenIdConnect("oidc", options =>
                //{
                //    //用于在OpenID Connect协议完成后使用cookie处理程序发出cookie
                //    options.SignInScheme = "Cookies";
                //    //identity server 4 的服务地址
                //    options.Authority = "http://localhost:5000";
                //    options.RequireHttpsMetadata = false;
                //    //通过clientid识别客户端
                //    options.ClientId = "mvc";
                //    //用于在Cookie中保存IdentityServer中的令牌
                //    options.SaveTokens = true;

                //})

                // 使用Hybrid Flow并添加API访问控制
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";

                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;

                    options.ClientId = "mvc";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code id_token";

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.Scope.Add("api1");
                    options.Scope.Add("offline_access");
                });

            //1. 
            services.AddHttpClient();
            //2. 指定HttpClient名字
            services.AddHttpClient("github", client =>
            {
                client.BaseAddress = new Uri("https://api.github.com/");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                // ...
            });
            //3.使用自定义的HttpClient类
            services.AddHttpClient<IGitHubClient,GitHubClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            //添加authentication中间件
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
