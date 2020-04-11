using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tutorial3.Middlewares;
using Tutorial3.Services;

namespace Tutorial3
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDbService, DbServiceImpl>();
            services.AddTransient<IStudentDbService, StudentDbServiceImpl>();
            services.AddControllers();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbService dbService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseHttpsRedirection();
            
            app.UseMiddleware<LoggingMiddleware>();
            
            app.Use(async (context, next) => {
                var index = context.Request.Headers["Index"].ToString();
                if (!context.Request.Headers.ContainsKey("Index"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("There is No Index In the Request");
                    return;
                }
                if (!dbService.IsThereStudentWithId(index))
                {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("There is No Student With This Id:" + index);
                        return;
                }
                await next();
            });
            
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
