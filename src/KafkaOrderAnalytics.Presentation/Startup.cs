using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.NameTranslation;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Configuration;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Repositories;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Services;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Contracts;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Converters;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Converters.Interfaces;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Handlers;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Serializers;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Services;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation;

public sealed class Startup
{
    private readonly IConfiguration _configuration;
    private static readonly INpgsqlNameTranslator Translator = new NpgsqlSnakeCaseNameTranslator();

    public Startup(IConfiguration configuration) => _configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddLogging();

        var connectionString = _configuration["ConnectionString"]!;
        services
            .AddFluentMigrator(
                connectionString,
                typeof(SqlMigration).Assembly); 

        var kafkaSettingsSection = _configuration.GetSection(nameof(KafkaSettings));
        services.Configure<KafkaSettings>(kafkaSettingsSection);
        
        services.AddKafkaHandler<Ignore, OrderEvent, ItemHandler>(
            valueDeserializer: new SystemTextJsonSerializer<OrderEvent>(
                new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                }));

        MapCompositeTypes();

        services
            .AddSingleton<IItemRepository, ItemRepository>(_ => new ItemRepository(connectionString))
            .AddSingleton<IDateTimeProvider, DateTimeProvider>()
            .AddSingleton<IOrderEventConverter, OrderEventConverter>();
        
        services.AddHostedService<KafkaBackgroundService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
    }
    
    private static void MapCompositeTypes()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var mapper = NpgsqlConnection.GlobalTypeMapper;
#pragma warning restore CS0618 // Type or member is obsolete
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        mapper.MapComposite<InventoryItemEntityV1>("inventory_item_v1", Translator);
        mapper.MapComposite<SellerSaleEntityV1>("seller_sale_v1", Translator);
    }
}
