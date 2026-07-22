using System.Reflection;
using CqrsExample.Contexts;
using CqrsExample.Features.Products.Abstractions;
using CqrsExample.Options;
using CqrsExample.Repositories;
using CqrsExample.Services;
using CqrsExample.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RabbitMQ.Client;

namespace CqrsExample;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitmqOptions>(configuration.GetSection(RabbitmqOptions.SectionName));

        services.AddDbContext<CommandDbContext>(options
            => options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var connectionString = configuration.GetConnectionString("Mongo");
            return new MongoClient(connectionString);
        });

        services.AddSingleton<IConnection>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RabbitmqOptions>>().Value;

            var factory = new ConnectionFactory
            {
                HostName = options.Host,
                Port = options.Port,
                UserName = options.Username,
                Password = options.Password,
                VirtualHost = options.VirtualHost
            };

            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        services.AddScoped<RabbitmqService>();
    
        services.AddScoped<OutboxRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<IProductWriteRepository, ProductWriteRepository>();

        services
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddHostedService<OutboxEventScanner>();
        services.AddHostedService<OutboxEventConsumer>();
        services.AddHostedService<OutboxEventCleaner>();

        return services;
    }
}