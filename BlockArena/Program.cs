using BlockArena.Database;
using BlockArena.Hubs;
using BlockArena.Interactors;
using BlockArena.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using BlockArena.Common.Models;
using BlockArena.Common.Ratings;
using BlockArena.Common.Interfaces;
using BlockArena.Middlewares;

namespace BlockArena
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            var env = builder.Environment;

            // 1. SERVICES

            var signalR = builder.Services.AddSignalR(options =>
            {
                options.AddFilter<ExceptionHubFilter>();
            });

            if (configuration["UseBackplane"]?.ToLower() == "true")
            {
                signalR.AddStackExchangeRedis(configuration["RedisConnectionString"]);
            }

            builder.Services.AddResponseCompression();
            builder.Services.AddControllersWithViews();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            builder.Services.AddSpaStaticFiles(options =>
            {
                options.RootPath = "ClientApp/build";
            });

            builder.Services.AddScoped<IRatingStorage, RedisRatingStorage>();
            builder.Services.AddScoped<IRatingHandler, RedisRatingProvider>();
            builder.Services.AddScoped<IRatingUpdater, RatingUpdater>();
            builder.Services.AddScoped<Func<Task<Rating>>>(sp => sp.GetService<IRatingHandler>().GetRating);
            builder.Services.AddScoped<IScorePipeline, ScorePipeline>();
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = new ConfigurationOptions
                {
                    EndPoints = { { "redis-16210.c56.east-us.azure.redns.redis-cloud.com", 16210 } },
                    User = "default",
                    Password = configuration["RedisSecretKey"]
                };

                return ConnectionMultiplexer.Connect(config);
            });


            builder.Services.AddSingleton<IMongoClient>(sp =>
                configuration["MongoConnectionString"] == null
                    ? null
                    : new MongoClient(configuration["MongoConnectionString"])
            );

            var objectSerializer = new ObjectSerializer(_ => true);
            BsonSerializer.RegisterSerializer(objectSerializer);

            builder.Services.AddSingleton<InMemoryGameRoomStorage>();
            builder.Services.AddScoped<MongoRoomStorage>();
            builder.Services.AddScoped<IRoomStorage>(sp =>
            {
                var mongoClient = sp.GetService<IMongoClient>();
                return mongoClient == null
                    ? sp.GetService<InMemoryGameRoomStorage>()
                    : sp.GetService<MongoRoomStorage>();
            });

            builder.Services.AddSingleton<ExceptionHubFilter>();

            // 2. BUILD

            var app = builder.Build();

            // 3. MIDDLEWARE

            app.UseMiddleware<TheIpLogger>();
            app.UseResponseCompression();
            app.UseRouting();
            app.UsePresonalExceptionHandler(env, app.Services.GetRequiredService<ILoggerFactory>());

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SentryDsn")))
            {
                app.UseSentryTracing();
            }

            app.UseMiddleware<HttpsProxyRedirection>();
            app.UseMiddleware<RelicIgnoreCreation>("/gameHub");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GameHub>("/gameHub", options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}"
                );
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer("start");
                }
            });

            // 4. RUN

#if !DEBUG
var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
app.Urls.Add("http://*:" + port);
#endif

            app.Run();
        }
    }
}