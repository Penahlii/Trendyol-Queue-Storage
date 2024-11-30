namespace TrendyolQueue.Services;

public interface IQueueService
{
    Task SendMessageAsync(string message);
    Task<string> ReceiveMessageAsync();
    Task DeleteMessageAsync(string messageId, string popReceipt);
    Task<bool> QueueExistsAsync();
    Task<int> GetMessageCountAsync();
    Task PopulateQueuesAsync(string discountCode, int count);
}
