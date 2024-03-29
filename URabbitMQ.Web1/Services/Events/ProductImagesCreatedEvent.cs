namespace URabbitMQ.Web1.Services.Events
{
    /// <summary>
    /// rabbitmq için ileti dizilerinden event ı temsil eder
    /// bir diğeri ise message dır.
    /// </summary>
    public class ProductImagesCreatedEvent
    {
        public string? ImageName { get; set; }
    }
}
