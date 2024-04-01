using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Drawing;
using System.Text;
using System.Text.Json;
using URabbitMQ.Web1.Services.Events;

namespace URabbitMQ.Web1.Services.Subs.BackgroundServices
{
    public class ImageWaterMarkProcessBackgroundServices : BackgroundService
    {
        private readonly RabbitMQClientService _rabbitmqClientService;
        private readonly ILogger<ImageWaterMarkProcessBackgroundServices> _logger;
        private IModel _channel;
        private readonly IWebHostEnvironment _environment;
        public ImageWaterMarkProcessBackgroundServices(RabbitMQClientService rabbitmqClientService
                    , ILogger<ImageWaterMarkProcessBackgroundServices> logger,
                        IWebHostEnvironment environment)
        {
            _rabbitmqClientService = rabbitmqClientService;
            _logger = logger;
            _environment = environment;
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitmqClientService.Connect();

            //boyut önemli değil,birer birer al,herbir subscriber ıma data gelsin
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // asenktron bir şekilde işlemi gerçekleştireceğiz.
            var consumer = new AsyncEventingBasicConsumer(_channel);

            //autoack false , mesaj okunduğunda otomatik silinmesin
            _channel.BasicConsume(queue: RabbitMQClientService.QueueName, autoAck: false, consumer: consumer);

            consumer.Received +=
                      (sender, evnt) =>
                      {
                          try
                          {
                              var receivedByteData = Encoding.UTF8.GetString(evnt.Body.ToArray());
                              var productImageCreatedEvent = JsonSerializer.Deserialize<ProductImagesCreatedEvent>(receivedByteData);

                              // görsele yazılacak yazıyı belirle
                              var textToPrint = "www.erolakgul.net";
                              // var olan görselin path ini al
                              var path = Path.Combine(_environment.WebRootPath, "Images", productImageCreatedEvent.ImageName);
                              // image i al
                              using var img = Image.FromFile(path);
                              // graphic formata al
                              using var graphic = Graphics.FromImage(img);
                              // font ayarla
                              var font = new Font(FontFamily.GenericMonospace, 32, FontStyle.Bold, GraphicsUnit.Pixel);
                              // görsele yazılacak yazının ölçüsü alınır
                              var textSize = graphic.MeasureString(textToPrint, font);
                              // renk seçilir
                              var color = Color.FromArgb(120, 200, 130, 110);
                              // yazma aygıtı alınır
                              var brush = new SolidBrush(color);
                              // konumlandırma
                              var position = new Point(img.Width - ((int)textSize.Width + 30), img.Height - ((int)textSize.Height + 30));

                              // yazdırma işlemini başlat
                              graphic.DrawString(s: textToPrint, font: font, brush: brush, point: position);
                              // yeni görseli kaydet
                              img.Save(filename: "/WaterMarks" + productImageCreatedEvent.ImageName);

                              //dispose et
                              img.Dispose();
                              graphic.Dispose();

                              //deliveryTag silinecek olan tag ismi
                              //multiple true ise memory de işlenmemiş ama rabibitmq ya gitmemiş tag leri de sil
                              // false olursa sadece ilgili mesajın durumunu rabbit e bildir
                              _channel.BasicAck(deliveryTag: evnt.DeliveryTag, multiple: false);
                          }
                          catch (Exception ex)
                          {
                              _logger.LogError(ex.Message);
                          }
                          return Task.CompletedTask;
                      };

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

    }
}
