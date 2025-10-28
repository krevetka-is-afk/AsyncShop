// using System.Text;
// using System.Text.Json;
//
// namespace PaymentsService.Services;
//
// public class OrderStatusClient
// {
//     private readonly HttpClient _httpClient;
//     private readonly string? _ordersServiceUrl;
//
//     public OrderStatusClient(HttpClient httpClient, IConfiguration configuration)
//     {
//         _httpClient = httpClient;
//         _ordersServiceUrl = configuration["OrdersService:BaseUrl"] ?? throw new InvalidOperationException("OrdersService URL not configured");
//     }
//
//     public async Task NotifyStatus(Guid orderId, string status)
//     {
//         var body = new
//         {
//             orderStatus = status == "Paid" ? 2 : 3,
//         };
//
//         var json = new StringContent(
//             JsonSerializer.Serialize(body),
//             Encoding.UTF8,
//             "application/json"
//         );
//         
//         var url = $"{_ordersServiceUrl}/orders/{orderId}/status";
//         
//         var response = await _httpClient.PostAsync(url, json);
//
//         Console.WriteLine(!response.IsSuccessStatusCode
//             ? $"[Notify] Не удалось обновить заказ {orderId}: {response.StatusCode}"
//             : $"[Notify] Статус заказа {orderId} → {status}");
//     }
// }