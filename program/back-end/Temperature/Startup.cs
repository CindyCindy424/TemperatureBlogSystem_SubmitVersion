using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace Temperature
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //���ӿ������
            services.AddCors();



            // ����Swagger
            //services.AddSwaggerGen(c =>
            //{
            //  c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Demo", Version = "v1" });
            //});
            #region ����Swagger
            services.AddSwaggerGen(options =>
            {
                /*options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Description = "【0908 v1】 新增修改：1）account头像相关接口"
                });*/

                options.EnableAnnotations();  //配置返回参数的注释
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1", Description = "【0917 v1】 1）修改account接口" });
                // ��ȡxml�ļ���
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                // ��ȡxml�ļ�·��
               // var xmlFile = "./Temperature.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                // ���ӿ�������ע�ͣ�true��ʾ��ʾ������ע��
               // var xmlPath = "./Temperature.xml";
                options.IncludeXmlComments(xmlPath, true);

                //配置token显示
                //Bearer 的scheme定义
                var securityScheme = new OpenApiSecurityScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    //参数添加在头部
                    In = ParameterLocation.Header,
                    //使用Authorize头部
                    Type = SecuritySchemeType.Http,
                    //内容为以 bearer开头
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };
                //把所有方法配置为增加bearer头部信息
                var securityRequirement = new OpenApiSecurityRequirement
                    {
                        {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "bearerAuth"
                                    }
                                },
                                new string[] {}
                        }
                    };

                //注册到swagger中
                options.AddSecurityDefinition("bearerAuth", securityScheme);
                options.AddSecurityRequirement(securityRequirement);
            });
            #endregion
            //services.AddControllers();
            //services.AddMvc();

            //����Mvc + json ���л�
            /*services.AddMvc(options => { options.EnableEndpointRouting = false; })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                        .AddNewtonsoftJson(options =>
                        {
                            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                            options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm";
                        });*/

            //����Token
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = "https://www.cnblogs.com/chengtian",  //Token�䷢��˭
                    ValidIssuer = "https://www.cnblogs.com/chengtian", //Token�䷢����
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SecureKeySecureKeySecureKeySecureKeySecureKeySecureKey"))
                };
            });
            services.AddControllers().AddNewtonsoftJson(option =>
                //忽略循环引用
                option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            );
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // ��������������Դ����
            app.UseCors(options =>
            {
                options.AllowAnyHeader();
                options.AllowAnyMethod();
                options.SetIsOriginAllowed(_ => true);
                options.AllowCredentials();
            });

            //开启静态资源访问
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), /*@"StaticFiles"*/@"BlogPics")),
                RequestPath = new PathString("/BlogPics"),
            });

            

            // ����Swagger�й��м��
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Demo v1");
            });


            app.UseRouting();

            //app.UseEndpoints(endpoints =>
            //{
            // endpoints.MapGet("/", async context =>
            //{
            //   await context.Response.WriteAsync("Hello World!");
            // });
            //});

            //app.UseMvc(routes =>
            //   {
            //    routes.MapRoute(
            //        name: "Default",
            //        template: "{controller}/{action}/{id?}",
            //        defaults: new { controller = "Home", action = "Index" }
            //    );
            //})

            //Token
            app.UseAuthentication();//��֤�м��
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapAreaControllerRoute(
                    name: "areas", "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

        }
    }
}
