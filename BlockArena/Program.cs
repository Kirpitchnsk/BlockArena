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
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.WebSockets;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Threading;

namespace BlockArena
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            var env = builder.Environment;

            // 1. Конфигурация Kestrel для WebSocket
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                serverOptions.AllowSynchronousIO = true;

                // Для локальной отладки HTTPS
                if (env.IsDevelopment())
                {
                    serverOptions.Listen(IPAddress.Loopback, 5000);
                    serverOptions.Listen(IPAddress.Loopback, 5001, listenOptions =>
                    {
                        listenOptions.UseHttps();
                    });
                }
            });

            // 2. Поддержка заголовков от прокси
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // 3. Добавление сервисов WebSocket
            builder.Services.AddWebSockets(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromMinutes(2);
            });

            // SignalR с увеличенными таймаутами
            var signalR = builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.AddFilter<ExceptionHubFilter>();
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
                options.HandshakeTimeout = TimeSpan.FromSeconds(30);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
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

            builder.Services.AddSingleton<InMemoryRoomStorage>();
            builder.Services.AddScoped<MongoRoomStorage>();
            builder.Services.AddScoped<IRoomStorage>(sp =>
            {
                var mongoClient = sp.GetService<IMongoClient>();
                return mongoClient == null
                    ? sp.GetService<InMemoryRoomStorage>()
                    : sp.GetService<MongoRoomStorage>();
            });

            builder.Services.AddSingleton<ExceptionHubFilter>();

            var app = builder.Build();

            // 4. Применение ForwardedHeaders в начале конвейера
            app.UseForwardedHeaders();

            app.UseMiddleware<TheIpLogger>();
            app.UseResponseCompression();
            app.UseRouting();
            app.UsePresonalExceptionHandler(env, app.Services.GetRequiredService<ILoggerFactory>());

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SentryDsn")))
            {
                app.UseSentryTracing();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // HSTS отключен для работы за прокси
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            // 5. Добавление поддержки WebSocket
            app.UseWebSockets();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GameHub>("/gameHub", options =>
                {
                    // Разрешаем WebSocket и LongPolling как fallback
                    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
                });

                // 6. Диагностический эндпоинт для WebSocket
                endpoints.MapGet("/ws-test", async context =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using var ws = await context.WebSockets.AcceptWebSocketAsync();
                        await Task.Delay(2000);
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", CancellationToken.None);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("WebSocket only");
                    }
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

            // 7. Настройка порта для Deploy-f
#if !DEBUG
var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
app.Urls.Add("http://*:" + port);

 Console.WriteLine($"Starting server on port {port}");
#endif

            app.Run();
        }
    }
}
