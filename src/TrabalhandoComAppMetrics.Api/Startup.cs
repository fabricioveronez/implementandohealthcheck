using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Health;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TrabalhandoComAppMetrics.Api
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
            var metrics = new MetricsBuilder()
                                .Report.ToInfluxDb(options =>
                                {
                                    options.InfluxDb.BaseUri = new Uri("http://127.0.0.1:8086");
                                    options.InfluxDb.Database = "appmetrics";                                    
                                    options.InfluxDb.UserName = "influxUser";
                                    options.InfluxDb.Password = "influxPwd";
                                    options.FlushInterval = TimeSpan.FromSeconds(30);
                                })
                                //.Report.ToConsole()
                                .Build();

            var health = AppMetricsHealth.CreateDefaultBuilder().Configuration.Configure(new HealthOptions() { Enabled = true, ApplicationName = "http://health.local.com", ReportingEnabled = true })   
            .Report.ToMetrics(metrics)       
            .HealthChecks.RegisterFromAssembly(services)                 
            .BuildAndAddTo(services);

            services.AddHealth(health);
            services.AddHealthEndpoints();

            services.AddMetrics(metrics);

            // ASP.NET CORE 2.1
            // Metricas
            services.AddMetricsReportingHostedService();
            // Healthcheck
            services.AddHealthReportingHostedService();

            // ASP.NET CORE 2.0
            //services.AddMetricsReportScheduler();

            services.AddMetricsTrackingMiddleware();

            services.AddMvc().AddMetrics();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();  
            }
            
            // To add all supported endpoints
            app.UseHealthAllEndpoints();
            app.UseMetricsAllMiddleware();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
