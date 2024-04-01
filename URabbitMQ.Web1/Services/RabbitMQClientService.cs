using RabbitMQ.Client;

namespace URabbitMQ.Web1.Services
{
    public class RabbitMQClientService : IDisposable
    {
        private readonly ILogger<RabbitMQClientService> _logger;

        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;

        public static string ExchangeName = "Image-DirectExchange";
        public static string RoutingKey = "route-watermark";
        public static string QueueName = "queue-watermark";

        public RabbitMQClientService(ConnectionFactory connectionFactory,ILogger<RabbitMQClientService> logger)
        {
            _logger = logger;
            // di dan gelen bilgi ile url dolu olacak
            _connectionFactory = connectionFactory;
        }

        public IModel Connect()
        {
            try
            {
                // bağlantı aç
                _connection = _connectionFactory.CreateConnection();

                // channel açık ise zaten onu dön
                if (_channel is { IsOpen: true })
                {
                    return _channel;
                }

                _channel = _connection.CreateModel();

                // exchange deklare edilir
                // exchange adı,tipi,durable, otosilmesin kaydetsin
                _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);

                //kuyruk deklare edilir
                // exclusive başka kanallardan buraya gelinemsin
                _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                //kuyruk bind edilr
                _channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingKey, arguments: null);

                _logger.LogInformation("Rabbitmq ile bağlantı kuruldu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }


            return _channel;
        }

        public void Dispose()
        {
            // kanal varsa kapat
            _channel?.Close();
            _channel?.Dispose();

            //bağlantı varsa
            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("Rabbitmq ile bağlantı koptu");
        }
    }
}
