using System.Text.Json.Serialization;
using System.Text.Json;
using Confluent.Kafka;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;
using Bogus;
using Microsoft.Extensions.Options;
using Ozon.Route256.Practice.OrdersService.Infrastructure.GrpcClients;
using Ozon.Route256.Practice.OrdersService.Application.Bll;
using Ozon.Route256.Practice.OrdersService.Domain;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;

public sealed class PreOrderConsumerService : ConsumerBackgroundService<long, string>
{
    private const int DeliveryArea = 5000000; // meters
    private static readonly Faker Faker = new();
    private Dictionary<string, (double latitude, double longitude)> _depotsByRegion = new();
    private readonly ILogger<PreOrderConsumerService> _logger;
    private readonly INewOrderProducer _producer;
    private readonly ICustomersServiceClient _customersServiceTest;
    private CachedCustomersClient _cachedCustomersClient;
    private IOrdersService _ordersService;
    private bool _isCustomersFilledTest = false;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public PreOrderConsumerService(
        IServiceProvider serviceProvider,
        INewOrderProducer producer,
        ICustomersServiceClient customersService,
        IOptions<KafkaConsumerSettings> kafkaSettings,
        ILogger<PreOrderConsumerService> logger)
        : base(serviceProvider,
            kafkaSettings,
            logger)
    {
        _logger = logger;
        _producer = producer;
        _customersServiceTest = customersService;
        KafkaConsumer = new KafkaConsumer(_kafkaSettings, "pre_orders_group");
    }

    protected override string TopicName { get; } = "pre_orders";
    protected override IKafkaConsumer<long, string> KafkaConsumer { get; }

    protected override async Task HandleAsync(
        IServiceProvider serviceProvider,
        ConsumeResult<long, string> message, 
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _cachedCustomersClient = serviceProvider.GetRequiredService<CachedCustomersClient>();
        _ordersService = serviceProvider.GetRequiredService<IOrdersService>();

        _logger.LogInformation("Handling messages from kafka {TopicName} {message.Message.Value}", TopicName, message.Message.Value);
        var value = message.Message.Value;
        var preOrder = JsonSerializer.Deserialize<PreOrder>(value, _jsonSerializerOptions);

        int count = 5;
        if (!_isCustomersFilledTest) // for testing. there's no customers in CustomersService todo
        {
            for( var i = 1; i <= count; i++)
            {
                var randomCustomer = CreateRandomCustomer(i);
                await _customersServiceTest.CreateCustomerAsync(randomCustomer, cancellationToken);
                _logger.LogInformation("Customers servise contains customers");
            }
            _isCustomersFilledTest = true;
        }
        var customerId = Random.Shared.Next(1, count + 1);// preOrder.Customer.Id testing purpose
        var customer = await _cachedCustomersClient.GetCustomerByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            _logger.LogError("Couldn't find a customer in CustomerService with id = {customerId} " +
                "from a preorder with key {message.Message.Key}", customerId, message.Message.Key);
            return;
        }
        var isExist = await _ordersService.IsOrderExistAsync(preOrder.Id, cancellationToken, internalRequest: true);
        if (isExist)
        {
            _logger.LogError("Coudn't handle preorder with id = {preOrder.Id} because repository already contains an order with the same Id.", preOrder.Id);
            return;
        }

        await MockPreorderRegionAsync(preOrder, cancellationToken);

        var orderEntity = Mappers.From(preOrder, customer);
        cancellationToken.ThrowIfCancellationRequested();
        await _ordersService.AddOrderAsync(orderEntity, cancellationToken);
        _logger.LogInformation("Preorder added");

        if (await ValidatePreOrderAsync(preOrder, cancellationToken))
        {
            await _producer.ProduceAsync(new[] { orderEntity.Order.Id }, cancellationToken);
        }
    }

    private async Task UploadRegionsIfNeededAsync(CancellationToken cancellationToken)
    {
        if (!_depotsByRegion.Any())
        {
            Region[] regions = await _ordersService.GetRegionsAsync(cancellationToken);
            foreach (var r in regions)
            {
                _depotsByRegion.TryAdd(r.Name, (r.Depot.Latitude, r.Depot.Longitude));
            }
        }
    }

    private async Task MockPreorderRegionAsync(// todo for testing purposes 
        PreOrder preOrder,
        CancellationToken cancellationToken)
    {
        await UploadRegionsIfNeededAsync(cancellationToken);
        var rand = new Random();
        preOrder.Customer.Address.Region = _depotsByRegion.ElementAt(rand.Next(0, _depotsByRegion.Count)).Key;
    }

    private async Task<bool>ValidatePreOrderAsync(
        PreOrder preOrder,
        CancellationToken cancellationToken)
    {
        await UploadRegionsIfNeededAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        var region = preOrder.Customer.Address.Region;
        if (_depotsByRegion.TryGetValue(region, out (double latitude, double longitude) depot))
        {
            var distance = GetDistance(depot, (preOrder.Customer.Address.Latitude, preOrder.Customer.Address.Longitude));
            var isValid = distance <= DeliveryArea;
            return isValid;
        }
        else
        {
            _logger.LogError("Preorder with id = {preOrder.Id} from a not valid region", preOrder.Id);
            return false;
        }
    }
   
    /// <summary>
    /// Distance between two points in meters (GeoCoordinate)
    /// </summary>
    /// <param name="address1"></param>
    /// <param name="address2"></param>
    /// <returns></returns>
    private static double GetDistance((double latitude, double longitude) address1, (double latitude, double longitude) address2)
    {
        //  The Haversine formula according to Dr. Math.
        //  http://mathforum.org/library/drmath/view/51879.html

        //  dlon = lon2 - lon1
        //  dlat = lat2 - lat1
        //  a = (sin(dlat/2))^2 + cos(lat1) * cos(lat2) * (sin(dlon/2))^2
        //  c = 2 * atan2(sqrt(a), sqrt(1-a)) 
        //  d = R * c

        //  Where
        //    * dlon is the change in longitude
        //    * dlat is the change in latitude
        //    * c is the great circle distance in Radians.
        //    * R is the radius of a spherical Earth.
        //    * The locations of the two points in 
        //        spherical coordinates (longitude and 
        //        latitude) are lon1,lat1 and lon2, lat2.

        var oneDegree = Math.PI / 180.0;
        var dLat1 = address1.latitude * oneDegree;
        var dLon1 = address1.longitude * oneDegree;
        var dLat2 = address2.latitude * oneDegree;
        var dLon2 = address2.longitude * oneDegree;

        double dLon = dLon2 - dLon1;
        double dLat = dLat2 - dLat1;

        // Intermediate result a.
        double a = Math.Pow(Math.Sin(dLat / 2.0), 2.0) +
                   Math.Cos(dLat1) * Math.Cos(dLat2) *
                   Math.Pow(Math.Sin(dLon / 2.0), 2.0);

        double greatCircleDist = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a)); //in Radians

        const double kEarthRadiusMs = 6376500;
        double dDistance = kEarthRadiusMs * greatCircleDist;

        return dDistance;
    }

    private static Proto.Customer CreateRandomCustomer(int id = -1)
    {
        var addresses = Enumerable.Range(0, Faker.Random.Int(1, 3)).
                Select(_ => new Proto.Address
                {
                    Apartment = Faker.Random.Int(9, 999).ToString(),
                    Building = Faker.Address.BuildingNumber(),
                    Region = Faker.Address.State(),
                    City = Faker.Address.City(),
                    Street = Faker.Address.StreetName(),
                    Latitude = Faker.Address.Latitude(),
                    Longitude = Faker.Address.Longitude()
                });
        return new Proto.Customer
        {
            Id = id == -1 ? Faker.Random.Int(100, 10000) : id,
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName(),
            MobileNumber = Faker.Phone.PhoneNumber(),
            Email = Faker.Person.Email,
            Addressed = { addresses },
            DefaultAddress = addresses.First()
        };
    }
}