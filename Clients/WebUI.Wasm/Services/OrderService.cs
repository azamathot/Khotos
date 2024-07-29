using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using SharedModels.Orders;
using SharedModels.Orders.Status;
using SharedModels.Payment;
using System.Net;
using WebUI_Wasm.Data;
using static WebUI_Wasm.Services.HttpService;

namespace WebUI_Wasm.Services
{
    public class OrderService
    {
        readonly HttpService _httpService;
        readonly UserService _userService;
        readonly UserManager<ApplicationUser> _userManager;
        readonly NavigationManager _navigationManager;
        readonly IConfiguration _config;

        public string adminEmail;

        public OrderService(HttpService httpService, UserManager<ApplicationUser> userManager, UserService userService, IConfiguration config, NavigationManager navigation)
        {
            _httpService = httpService;
            _userManager = userManager;
            _userService = userService;
            _config = config;
            _navigationManager = navigation;
            adminEmail = _config.GetSection("AdminUser")["AdminEmail"] ?? string.Empty;
        }

        public async Task<IEnumerable<OrderView>> GetAllOrdersAsync()
        {
            var response = await _httpService.HttpClient(ServicesUrl.OrderingAPI).GetAsync($"/ordering/allorders/{_userService!.UserId}", CancellationToken.None);
            if (response != null && response.IsSuccessStatusCode == true)
            {
                JsonSerializerSettings settings = new()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                };
                var json = await response.Content.ReadAsStringAsync();
                var orderList = JsonConvert.DeserializeObject<IEnumerable<OrderView>>(json, settings)!;
                return _userManager.Users.ToList().Join(
                    orderList,
                    u => u.Id,
                    o => o.Order.UserId,
                    (u, ov) => new OrderView()
                    {
                        Order = ov.Order,
                        CountNewMessage = ov.CountNewMessage,
                        LastChange = ov.LastChange ?? ov.Order.Created
                    }).OrderByDescending(x => x.LastChange).ToList();
            }
            return new List<OrderView>();
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            if (order.Price > 0 && order.StatusPayment == (int)KnownPaymentStatus.NotAssigned)
                order.StatusPayment = (int)KnownPaymentStatus.NotPaid;
            var response = await _httpService.HttpClient(ServicesUrl.OrderingAPI).PutAsJsonAsync($"/ordering/{order.Id}", order);
            return response != null && response.IsSuccessStatusCode == true;
        }

        public async Task<IEnumerable<OrderView>> GetCustomerOrdersAsync()
        {
            var response = await _httpService.HttpClient(ServicesUrl.OrderingAPI).GetAsync($"/ordering/ordersOfUser/{_userService!.UserId}", CancellationToken.None);
            if (response != null && response.IsSuccessStatusCode == true)
            {
                JsonSerializerSettings settings = new()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                };
                var json = await response.Content.ReadAsStringAsync();
                var orderUserName = _userManager.Users.FirstOrDefault(x => x.Id == _userService!.UserId)!.UserName;
                var orderList = JsonConvert.DeserializeObject<IEnumerable<OrderView>>(json, settings)!;
                _ = orderList.All(m =>
                {
                    m.LastChange ??= m.Order.Created;
                    return true;
                });
                return orderList.OrderByDescending(x => x.LastChange).ToList();
            }
            return new List<OrderView>();
        }

        public async Task<HttpStatusCode> AddNewOrderAsync(Order order)
        {
            var response = await _httpService.HttpClient(ServicesUrl.OrderingAPI).PostAsJsonAsync("ordering", order);
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
            var response = await _httpService.HttpClient(ServicesUrl.PaynmentAPI).PostAsJsonAsync("payment/robokassapayment/paymentorder", request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var link = await response.Content.ReadAsStringAsync();
                _navigationManager.NavigateTo(link);
            }
            return response.StatusCode;
        }

        public async Task UpdateOrderPaymentStatusAsync(int orderId, int status)
        {
            var response = await _httpService.HttpClient(ServicesUrl.OrderingAPI).PutAsync($"ordering/UpdateOrderPaymentStatus/{orderId}/{status}", null);
            if (response.StatusCode == HttpStatusCode.OK) { }
        }
    }
}
