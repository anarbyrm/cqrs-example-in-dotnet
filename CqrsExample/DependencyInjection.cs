using System.Reflection;
using CqrsExample.Contexts;
using CqrsExample.Features.Products.Abstractions;
using CqrsExample.Repositories;
using CqrsExample.Workers;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace CqrsExample;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CommandDbContext>(options 
            => options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var connectionString = configuration.GetConnectionString("Mongo");
            return new MongoClient(connectionString);
        });

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