using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using parkingbot.Models;

namespace parkingbot
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("vcap-local.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            var vcapServices = Environment.GetEnvironmentVariable("VCAP_SERVICES");

            if (vcapServices != null)
            {
                dynamic json = JsonConvert.DeserializeObject(vcapServices);

                if (json.cleardb != null)
                {
                    try
                    {
                        Configuration["cleardb:0:credentials:uri"] = json.cleardb[0].credentials.uri;
                    }
                    catch
                    {
                        Console.WriteLine("Failed to read ClearDB uri, ignore this and continue without a database.");
                    }
                }
            }
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var databaseUri = Configuration["cleardb:0:credentials:uri"];

            if (!string.IsNullOrEmpty(databaseUri))
            {
                // add database context
                services.AddDbContext<ParkingBotDbContext>(
                    options => options.UseMySql(GetConnectionString(databaseUri))
                );
            }

            // Add framework services.
            services.AddMvc();
        }

        private static string GetConnectionString(string databaseUri)
        {
            string connectionString;

            try
            {
                var username = databaseUri.Split('/')[2].Split(':')[0];
                var password = databaseUri.Split(':')[2].Split('@')[0];
                var portSplit = databaseUri.Split(':');
                var port = portSplit.Length == 4 ? portSplit[3].Split('/')[0] : null;
                var hostSplit = databaseUri.Split('@')[1];
                var hostname = port == null ? hostSplit.Split('/')[0] : hostSplit.Split(':')[0];
                var databaseSplit = databaseUri.Split('/');
                var database = databaseSplit.Length == 4 ? databaseSplit[3] : null;
                var optionsSplit = database?.Split('?');
                database = optionsSplit.First();
                port = port ?? "3306"; // if port is null, use 3306
                connectionString = $"Server={hostname};uid={username};pwd={password};Port={port};Database={database};SSL Mode=Required;";
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new FormatException("Invalid database uri format", ex);
            }

            return connectionString;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/migrations

            // dotnet ef migrations add ParkingBotDb
            // dotnet ef database update

            var context = app.ApplicationServices.GetService(typeof(ParkingBotDbContext)) as ParkingBotDbContext;
            context?.Database.EnsureCreated();

            app.UseMvc();
        }
    }

    public class ParkingBotDbContext : DbContext
    {
        public ParkingBotDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Availability> Availability { get; set; }
        public DbSet<Logs> Logs { get; set; }
    }
}