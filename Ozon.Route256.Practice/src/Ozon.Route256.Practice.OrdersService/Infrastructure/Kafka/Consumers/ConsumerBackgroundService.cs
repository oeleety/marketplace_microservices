using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Ozon.Route256.Practice.OrdersService.Configuration;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;

public abstract class ConsumerBackgroundService<TKey, TValue> : BackgroundService
{
    private readonly ILogger _logger;
    protected readonly IServiceScope _scope;
    protected readonly IOptions<KafkaSettings> _kafkaSettings;

    protected ConsumerBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<KafkaSettings> kafkaSettings,
        ILogger logger)
    {
        _logger = logger;
        _scope = serviceProvider.CreateScope();
        _kafkaSettings = kafkaSettings;
    }

    protected abstract string TopicName { get; }
    protected abstract IKafkaConsumer<TKey, TValue> KafkaConsumer { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        if (stoppingToken.IsCancellationRequested)
        {
            KafkaConsumer.Consumer.Close();
            return;
        }

        KafkaConsumer.Consumer.Subscribe(TopicName);

        _logger.LogInformation("{KafkaConsumer.Consumer.Name} start consumimg topic {Topic}", KafkaConsumer.Consumer.Name, TopicName);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ConsumeAsync(stoppingToken);
        }
        KafkaConsumer.Consumer.Close();
        KafkaConsumer.Consumer.Unsubscribe();

        _logger.LogInformation("Stop consumer topic {Topic}", TopicName);
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        ConsumeResult<TKey, TValue>? message = null;

        try
        {
            message = KafkaConsumer.Consumer.Consume(TimeSpan.FromMilliseconds(100));

            if (message is null)
            {
                await Task.Delay(100, cancellationToken);
                return;
            }

            await HandleAsync(message, cancellationToken);
            KafkaConsumer.Consumer.Commit();
        }
        catch (Exception exc)
        {
            var key = message is not null ? message.Message.Key!.ToString() : "No key";
            var value = message is not null ? message.Message.Value!.ToString() : "No value";

            _logger.LogError(exc, "Error process message Key:{Key} Value:{Value}", key, value);
        }
    }

    protected abstract Task HandleAsync(ConsumeResult<TKey, TValue> message, CancellationToken cancellationToken);

    public override void Dispose()
    {
        KafkaConsumer?.Consumer?.Close();
        _scope.Dispose();
        base.Dispose();
    }
}