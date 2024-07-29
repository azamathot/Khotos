using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using SharedModels.Chats;
using SharedModels.Orders;
using System.Net;
using WebUI_Wasm.Hubs;
using static WebUI_Wasm.Services.HttpService;

namespace WebUI_Wasm.Services
{
    [Authorize]
    public class ChatService
    {
        public const long MaxFileSize = 1024 * 1024 * 2;

        private HubConnection? _hubConnection;
        private readonly IConfiguration _config;
        private readonly HttpService _httpService;
        private readonly UserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private string adminEmail;
        private CookieContainer container;
        private Dictionary<string, string?> headers;
        private Uri url;

        public delegate Task EventUserUpdate(Dictionary<string, string> users);
        public event Action<ChatMessage> OnMessageDataReceived;
        public event EventUserUpdate OnUsersUpdate;

        public Dictionary<string, string> UserList { get; set; }
        public string Companion { get; set; }
        public ChatService(HttpService httpService, IConfiguration config, UserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _httpService = httpService;
            _config = config;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            Initialize();
        }

        private void Initialize()
        {
            adminEmail = _config.GetSection("AdminUser")["AdminEmail"] ?? string.Empty;
            container = new CookieContainer();
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
            {
                foreach (var item in request.Cookies)
                {
                    var cookie = new Cookie()
                    {
                        Name = item.Key,
                        Domain = "localhost",
                        Value = item.Value
                    };
                    container.Add(cookie);
                }

                headers = new Dictionary<string, string?>();
                foreach (var item in request.Headers)
                {
                    headers.Add(item.Key, item.Value);
                }
            }
            url = new Uri(_httpService.HttpClient(ServicesUrl.webui_wasm).BaseAddress!, ChatHub.HubUrl);
        }

        public async Task StartChatAsync(OrderView? orderView)
        {
            if (orderView != null && _userService!.UserId != null)
            {
                headers.Remove("groupname");
                headers.Add("groupname", orderView.Order.Id.ToString());
                Companion = orderView.Order.Username.Equals(_userService!.UserName) ? adminEmail : orderView.Order.Username;
            }
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    //options.AccessTokenProvider = ()=>Task.FromResult(_my)
                    options.UseDefaultCredentials = true;
                    options.Cookies = container;
                    options.Headers = headers!;
                    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (env != null && !env.Equals("Development"))
                    {
                        return;
                    }
                    options.HttpMessageHandlerFactory = (x) => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                    };
                }).Build();

            _hubConnection.On<Dictionary<string, string>>("UpdateUsersAsync", users =>
            {
                UserList = users;
                OnUsersUpdate?.Invoke(users);
            });
            _hubConnection.On<ChatMessage>("SendMessageAsync", (messageData) =>
            {
                OnMessageDataReceived?.Invoke(messageData);
            });
            await _hubConnection.StartAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetChatsByOrderIdAsync(int orderId)
        {
            var response = await _httpService.HttpClient(ServicesUrl.OrderingAPI).GetAsync($"ordering/chat/{orderId}/{_userService!.UserId}");
            if (response != null && response.IsSuccessStatusCode == true)
            {
                JsonSerializerSettings settings = new()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                };
                var json = await response.Content.ReadAsStringAsync();
                var orderChatList = JsonConvert.DeserializeObject<IEnumerable<ChatMessage>>(json, settings)!;
                _ = orderChatList.All(m =>
                {
                    m.Mine = m.UserId.Equals(_userService!.UserId);
                    m.UserName ??= m.UserId.Equals(_userService!.UserId) ? _userService!.UserName : adminEmail;
                    //m.IsReaded = m.IsReaded || !m.UserId.Equals(CurrentUser.Id);
                    return true;
                });
                return orderChatList;
            }
            return new List<ChatMessage>();
        }

        public async Task SendMessageDataAsync(string? message, byte[]? mediaData = null, string? mediaType = null, string? mediaFilename = null, int? orderId = null)
        {
            var sendMessageData = new ChatMessage()
            {
                UserId = _userService.UserId!,
                SendTime = DateTimeOffset.Now,
                OrderId = orderId,
                IsReaded = false,
                Message = message,
                MediaData = GetMessageHtml(mediaData, mediaType, mediaFilename),
                UserName = _userService.UserName
            };
            if (orderId != null && orderId > 0)
                await _httpService.HttpClient(ServicesUrl.OrderingAPI).PostAsJsonAsync<ChatMessage>("ordering/chat", sendMessageData);
            await _hubConnection!.SendAsync("SendMessageAsync", sendMessageData);
        }

        public async Task UpdateOrderChat(ChatMessage orderChat)
        {
            var resp = await _httpService.HttpClient(ServicesUrl.OrderingAPI).PutAsJsonAsync($"ordering/chat/{orderChat.Id}", orderChat);
        }
        public async Task DisconnectAsync()
        {
            try
            {
                await _hubConnection!.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
            catch { }
        }

        private static string GetMessageHtml(byte[]? mediaData, string? mediaType, string? mediaFilename)
        {
            string msg = string.Empty;
            if (mediaData != null)
            {
                var mediaUrl = $"data:{mediaType};base64,{Convert.ToBase64String(mediaData)}";
                if (mediaType!.StartsWith("image/"))
                    msg = $"<br/><img src = \"{mediaUrl}\" height = \"100\" />";
                else if (mediaType.StartsWith("audio/"))
                    msg = $"<br/><audio controls src = \"{mediaUrl}\" />";
                else if (mediaType.StartsWith("video/"))
                    msg = $"<br/><video controls src = \"{mediaUrl}\" />";
                else
                    msg = $"<br/><a href = \"{mediaUrl}\" download=\"{mediaFilename}\" > {mediaFilename} </a>";
            }
            return msg;
        }

    }
}
