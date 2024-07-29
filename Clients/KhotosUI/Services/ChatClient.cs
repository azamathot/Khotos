using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using SharedModels.Chats;
using SharedModels.Orders;

namespace KhotosUI.Services
{
    [Authorize]
    public class ChatClient
    {
        private const string Admin = "azamathot";

        private HubConnection? _hubConnection;
        private readonly HttpClient _httpClientApi;
        private readonly UserService _userService;
        public List<ChatMessage> Messages { get; set; }
        private Uri url;

        public event Action OnMessageDataReceived;
        public string Companion { get; set; }
        public bool IsCompanionOnline { get; set; }
        public ChatClient(IHttpClientFactory httpClientFactory, UserService userService)
        {
            _httpClientApi = httpClientFactory.CreateClient("API");
            _userService = userService;
            Initialize();
        }

        private void Initialize()
        {
            Messages = new List<ChatMessage>();
            //url = new Uri(new Uri("https://localhost:7005"), HubNames.HubUrl);
            url = new Uri(_httpClientApi.BaseAddress!, HubNames.HubUrl);
        }

        [Authorize]
        public async Task StartChatAsync(OrderView? orderView)
        {
            Messages.Clear();
            Messages.AddRange(await GetChatsByOrderIdAsync(orderView.Order.Id));
            Companion = orderView.Order.Username.Equals(_userService!.UserName) ? Admin : orderView.Order.Username;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url + $"?access_token={JwtAuthorizationMessageHandler.JwtToken}", options =>
                {
                    //options.Headers.Add("Authoriazation", "Bearer " + JwtAuthorizationMessageHandler.JwtToken);
                    options.AccessTokenProvider = () => Task.FromResult(JwtAuthorizationMessageHandler.JwtToken);
                    //options.SkipNegotiation = true;
                    //options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                })
                .Build();
            _hubConnection.On<ChatMessage>(HubNames.RECEIVE, async (messageData) =>
            {
                messageData.Mine = messageData.UserId.Equals(_userService.UserId, StringComparison.OrdinalIgnoreCase);
                messageData.IsReaded = messageData.IsReaded || !messageData.UserId.Equals(_userService.UserId);
                Messages.Add(messageData);
                await IsReadedMsg(messageData);
                OnMessageDataReceived?.Invoke();
            });
            _hubConnection.On<int>(HubNames.DELETE_MESSAGE, async (messageId) =>
            {
                var msg = Messages.FirstOrDefault(x => x.Id == messageId);
                if (msg != null)
                    Messages.Remove(msg);
                OnMessageDataReceived?.Invoke();
            });
            _hubConnection.On<int>(HubNames.IS_READED, async (messageId) =>
            {
                var msg = Messages.FirstOrDefault(x => x.Id == messageId);
                if (msg != null)
                    msg.IsReaded = true;
                OnMessageDataReceived?.Invoke();
            });
            _hubConnection.On<ChatMessage>(HubNames.UPDATE_MESSAGE, async (messageData) =>
            {
                var msg = Messages.FirstOrDefault(x => x.Id == messageData.Id);
                msg.Message = messageData.Message;
                msg.MediaData = messageData.MediaData;
                msg.IsReaded = messageData.IsReaded;
                OnMessageDataReceived?.Invoke();
            });
            _hubConnection.On<string>(HubNames.JOIN_GROUP, async (userName) =>
            {
                if (userName == Companion)
                    IsCompanionOnline = true;
                OnMessageDataReceived?.Invoke();
            });
            _hubConnection.On<string>(HubNames.LEAVE_GROUP, async (userName) =>
            {
                if (userName == Companion)
                    IsCompanionOnline = false;
                OnMessageDataReceived?.Invoke();
            });
            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync(HubNames.JOIN_GROUP,
                orderView.Order.Id.ToString(),
                _userService.UserName,
                _userService.UserId, Companion);
        }

        private async Task<IEnumerable<ChatMessage>> GetChatsByOrderIdAsync(int orderId)
        {
            var response = await _httpClientApi.GetAsync($"chat/{orderId}");
            if (response != null && response.IsSuccessStatusCode == true)
            {
                JsonSerializerSettings settings = new()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                };
                var json = await response.Content.ReadAsStringAsync();
                var orderChatList = JsonConvert.DeserializeObject<IEnumerable<ChatMessage>>(json, settings)!;
                return orderChatList;
            }
            return new List<ChatMessage>();
        }

        public async Task SendMessageDataAsync(string? message, byte[]? mediaData = null, string? mediaType = null, string? mediaFilename = null, int? orderId = null)
        {
            var messageData = new ChatMessage()
            {
                UserId = _userService.UserId!,
                SendTime = DateTimeOffset.Now,
                OrderId = orderId,
                IsReaded = false,
                Message = message,
                MediaData = GetMessageHtml(mediaData, mediaType, mediaFilename),
                UserName = _userService.UserName
            };
            messageData.Mine = true;
            Messages.Add(messageData);
            await _hubConnection!.SendAsync(HubNames.SEND, messageData);
            //OnMessageDataReceived?.Invoke();
        }

        public async Task UpdateChatMessage(ChatMessage message)
        {
            await _hubConnection.SendAsync(HubNames.UPDATE_MESSAGE, message);
        }

        private async Task IsReadedMsg(ChatMessage message)
        {
            await _hubConnection.SendAsync(HubNames.IS_READED, message.Id, message.OrderId);
        }

        public async Task DeleteMsg(ChatMessage message)
        {
            await _hubConnection.SendAsync(HubNames.DELETE_MESSAGE, message.Id, message.OrderId);
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
