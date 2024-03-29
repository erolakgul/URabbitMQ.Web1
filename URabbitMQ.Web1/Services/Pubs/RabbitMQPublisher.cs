using System.Text;
using System.Text.Json;
using URabbitMQ.Web1.Services.Events;

namespace URabbitMQ.Web1.Services.Pubs
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQClientService _rabbitMQClientService;

        public RabbitMQPublisher(RabbitMQClientService client)
        {
            _rabbitMQClientService = client;
        }

        public void Publish(ProductImagesCreatedEvent message)
        {
            var channel = _rabbitMQClientService.Connect();

            var bodyString = JsonSerializer.Serialize(message);
            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            // memory  de kalmasın kayıtlı dursun 
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            // mesajı punlisher ile rabbit e gönderiyoruz.
            channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName, routingKey: RabbitMQClientService.RoutingKey, mandatory: true, basicProperties: properties, body: bodyByte);

        }
    }
}
