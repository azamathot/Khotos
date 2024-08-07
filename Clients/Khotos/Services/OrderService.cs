using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using SharedModels.Orders;
using SharedModels.Orders.Status;
using SharedModels.Payment;
using System.Net;

namespace Khotos.Services
{
    [Authorize]
    public class OrderService
    {
        readonly HttpClient _httpclient;
        readonly UserService _userService;
        readonly NavigationManager _navigationManager;


        public OrderService(IHttpClientFactory httpService, UserService userService, NavigationManager navigation)
        {
            _httpclient = httpService.CreateClient("API");
            _userService = userService;
            _navigationManager = navigation;
        }

        [Authorize(Roles = "admin")]
        public async Task<IEnumerable<OrderView>> GetAllOrdersAsync()
        {
            var response = await _httpclient.GetAsync($"/ordering/getallorders", CancellationToken.None);
            if (response != null && response.IsSuccessStatusCode == true)
            {
                JsonSerializerSettings settings = new()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                };
                var json = await response.Content.ReadAsStringAsync();
                var orderList = JsonConvert.DeserializeObject<IEnumerable<OrderView>>(json, settings)!;

                return orderList.OrderByDescending(x => x.LastChange).ToList();
            }
            return new List<OrderView>();
        }

        [Authorize(Roles = "admin")]
        public async Task<bool> UpdateOrderAsync(Order order)
        {
            if (order.Price > 0 && order.StatusPayment == (int)KnownPaymentStatus.NotAssigned)
                order.StatusPayment = (int)KnownPaymentStatus.NotPaid;
            var response = await _httpclient.PutAsJsonAsync($"/ordering/{order.Id}", order);
            return response != null && response.IsSuccessStatusCode == true;
        }

        public async Task<IEnumerable<OrderView>> GetCustomerOrdersAsync()
        {
            var response = await _httpclient.GetAsync($"/ordering/getcustomerorders", CancellationToken.None);
            if (response != null && response.IsSuccessStatusCode == true)
            {
                JsonSerializerSettings settings = new()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                };
                var json = await response.Content.ReadAsStringAsync();
                var orderList = JsonConvert.DeserializeObject<IEnumerable<OrderView>>(json, settings)!;
                return orderList.OrderByDescending(x => x.LastChange).ToList();
            }
            return new List<OrderView>();
        }

        public async Task<HttpStatusCode> AddNewOrderAsync(Order order)
        {
            var response = await _httpclient.PostAsJsonAsync("ordering", order);
            return response.StatusCode;
        }

        public async Task<HttpStatusCode> PayClickAsync(OrderView orderView)
        {
            PaymentRequest request = new PaymentRequest()
            {
                OrderId = orderView.Order.Id,
                Username = orderView.Order.Username,
                Email = orderView.Order.Username,
                ProductId = orderView.Order.ProductId,
                ProductName = orderView.Order.ProductName,
                ProductDescription = orderView.Order.Description,
                Price = orderView.Order.Price,
                Quantity = 1
            };
            var response = await _httpclient.PostAsJsonAsync("payment/robokassapayment/paymentorder", request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var link = await response.Content.ReadAsStringAsync();
                _navigationManager.NavigateTo(link);
            }
            return response.StatusCode;
        }

        public async Task UpdateOrderPaymentStatusAsync(int orderId, int status)
        {
            var response = await _httpclient.PutAsync($"ordering/UpdateOrderPaymentStatus/{orderId}/{status}", null);
            if (response.StatusCode == HttpStatusCode.OK) { }
        }
    }
}
