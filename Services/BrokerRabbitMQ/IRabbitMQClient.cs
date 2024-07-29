namespace BrokerRabbitMQ
{
    public interface IRabbitMQClient
    {
        void PublishMessage(string queueName, string message);
        //  Добавляйте  другие  методы  по  нужде,  например,  получение  сообщения: 
        //  Task<string> GetMessage(string queueName); 
    }
}
