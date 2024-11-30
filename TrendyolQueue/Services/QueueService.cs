using Azure.Storage.Queues;

namespace TrendyolQueue.Services;

public class QueueService : IQueueService
{
    private readonly QueueClient _queueClient;
    private readonly QueueClient _countQueueClient;

    public QueueService(string connectionString, string discountQueueName, string countQueueName)
    {
        _queueClient = new QueueClient(connectionString, discountQueueName);
        _countQueueClient = new QueueClient(connectionString, countQueueName);

        _queueClient.CreateIfNotExists();
        //_countQueueClient.CreateIfNotExists();
    }

    public async Task SendMessageAsync(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            await _queueClient.SendMessageAsync(message, TimeSpan.FromSeconds(15), TimeSpan.FromMinutes(10));
        }
    }

    public async Task<string?> ReceiveMessageAsync()
    {
        var messageResponse = await _queueClient.ReceiveMessageAsync();
        if (messageResponse.Value != null)
        {
            return messageResponse.Value.Body.ToString();
        }
        return null;
    }

    public async Task DeleteMessageAsync(string messageId, string popReceipt)
    {
        if (!string.IsNullOrEmpty(messageId) && !string.IsNullOrEmpty(popReceipt))
        {
            await _queueClient.DeleteMessageAsync(messageId, popReceipt);
        }
    }

    public async Task<bool> QueueExistsAsync()
    {
        return await _queueClient.ExistsAsync();
    }

    public async Task<int> GetMessageCountAsync()
    {
        var properties = await _queueClient.GetPropertiesAsync();
        return properties.Value.ApproximateMessagesCount;
    }

    public async Task PopulateQueuesAsync(string discountCode, int count)
    {
        await _queueClient.ClearMessagesAsync();
        await _countQueueClient.ClearMessagesAsync();
        await SendMessageAsync(discountCode);
        await _countQueueClient.SendMessageAsync(count.ToString());
    }
}
