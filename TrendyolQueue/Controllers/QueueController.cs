using Microsoft.AspNetCore.Mvc;
using TrendyolQueue.Models;
using TrendyolQueue.Services;

namespace TrendyolQueue.Controllers;

public class QueueController : Controller
{
    private readonly IQueueService _queueService;

    public QueueController(IQueueService queueService)
    {
        _queueService = queueService;
    }

    public async Task<IActionResult> PopulateQueues()
    {
        string discountCode = "GBHG122435"; 
        int count = 10; 
        await _queueService.PopulateQueuesAsync(discountCode, count);

        TempData["Message"] = $"Queues populated with discount code {discountCode} and count {count}.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index()
    {
        bool queueExists = await _queueService.QueueExistsAsync();
        string? discountCode = null;
        int remainingCount = 0;

        if (queueExists)
        {
            discountCode = await _queueService.ReceiveMessageAsync();
            string? countMessage = await _queueService.ReceiveMessageAsync();

            if (!string.IsNullOrEmpty(countMessage))
            {
                remainingCount = int.Parse(countMessage) - 1;

                if (remainingCount > 0)
                {
                    await _queueService.SendMessageAsync(remainingCount.ToString());
                }
                else
                {
                    await _queueService.DeleteMessageAsync(discountCode, string.Empty);
                }
            }
        }

        return View(new QueueViewModel
        {
            DiscountCode = discountCode,
            RemainingCount = remainingCount,
            QueueExists = queueExists
        });
    }

    [HttpPost]
    public async Task<IActionResult> ApplyDiscount(string discountCode)
    {
        TempData["Message"] = $"Discount code {discountCode} applied successfully!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveDiscount(string discountCode, int remainingCount)
    {
        if (!string.IsNullOrEmpty(discountCode) && remainingCount > 0)
        {
            await _queueService.SendMessageAsync(discountCode);
            await _queueService.SendMessageAsync(remainingCount.ToString());
            TempData["Message"] = $"Discount code {discountCode} restored successfully!";
        }
        else
        {
            TempData["Message"] = "No discount code available to remove.";
        }

        return RedirectToAction("Index");
    }
}
