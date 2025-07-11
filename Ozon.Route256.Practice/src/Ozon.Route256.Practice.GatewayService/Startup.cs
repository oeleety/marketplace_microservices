﻿using System.Text.Json.Serialization;
using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Ozon.Route256.Practice.GatewayService.Middleware;
using Ozon.Route256.Practice.OrdersService.Proto;
using Ozon.Route256.Practice.Proto;
using Ozon.Route256.Practice.Shared;

namespace Ozon.Route256.Practice.GatewayService;

public sealed class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        var factory = new StaticResolverFactory(address => new[]
{
             new BalancerAddress("orders-service-1", 5005),
             new BalancerAddress("orders-service-2", 5005)
        });

        serviceCollection.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        serviceCollection.AddEndpointsApiExplorer();
        serviceCollection.AddSwaggerGen();
        serviceCollection.AddSingleton<ResolverFactory>(factory);
        serviceCollection.AddGrpcClient<Orders.OrdersClient>(options =>
        {
            options.Address = new Uri("static:///orders-service");
        }).ConfigureChannel(x =>
        {
            x.Credentials = ChannelCredentials.Insecure;
            x.ServiceConfig = new ServiceConfig
            {
                LoadBalancingConfigs = { new LoadBalancingConfig("round_robin") }
            };
        });

        serviceCollection.AddGrpcClient<Customers.CustomersClient>(option =>
        {
            option.Address = new Uri(_configuration.TryGetValue("ROUTE256_CUSTOMER_ADDRESS"));
        });
    }

    public void Configure(IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseMiddleware<ExceptionHandler>();
        applicationBuilder.UseRouting();
        applicationBuilder.UseSwagger();
        applicationBuilder.UseSwaggerUI();
        applicationBuilder.UseEndpoints(x => x.MapControllers());
    }
}
