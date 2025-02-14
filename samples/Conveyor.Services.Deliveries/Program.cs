﻿using Convey;
using Convey.CQRS.Events;
using Convey.Discovery.Consul;
using Convey.LoadBalancing.Fabio;
using Convey.Logging;
using Convey.MessageBrokers.CQRS;
using Convey.MessageBrokers.RabbitMQ;
using Convey.Metrics.Prometheus;
using Convey.Persistence.Redis;
using Convey.Tracing.Jaeger;
using Convey.Tracing.Jaeger.RabbitMQ;
using Convey.WebApi;
using Conveyor.Services.Deliveries;
using Conveyor.Services.Deliveries.Events.External;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

await CreateHostBuilder(args).Build().RunAsync();

static IHostBuilder CreateHostBuilder(string[] args)
    => Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder
                .ConfigureServices(services =>
                    services
                        .AddConvey()
                        .AddErrorHandler<ExceptionToResponseMapper>()
                        .AddConsul()
                        .AddFabio()
                        .AddJaeger()
                        .AddEventHandlers()
                        .AddRedis()
                        .AddRabbitMq(plugins: p => p.AddJaegerRabbitMqPlugin())
                        .AddPrometheus()
                        .AddWebApi())
                .Configure(app =>
                    app
                        .UseErrorHandler()
                        .UsePrometheus()
                        .UseRouting()
                        .UseEndpoints(endpoints =>
                            endpoints
                                .Get("", ctx => ctx.Response.WriteAsync("Deliveries Service"))
                                .Get("ping", ctx => ctx.Response.WriteAsync("pong")))
                        .UseJaeger()
                        .UseRabbitMq()
                        .SubscribeEvent<OrderCreated>());
        })
        .UseLogging();