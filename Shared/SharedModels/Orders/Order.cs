using SharedModels.Chats;
using System.ComponentModel.DataAnnotations;

namespace SharedModels.Orders
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public int ProductId { get; set; }

        [Display(Name = "Название услуги")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Необходимо ввести тему сообщения")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Длина имени должна быть от {2} до {1} символов")]
        [Display(Name = "Тема")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Необходимо ввести краткое описание заказа")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Длина имени должна быть от {2} до {1} символов")]
        [Display(Name = "Описание")]
        public string Description { get; set; }
        public int Price { get; set; } = 0;
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;

        [Range(minimum: 0, maximum: 100, ErrorMessage = "Значение готовности от {1} до {2} в %")]
        public int ProgressPercent { get; set; } = 0;
        public DateOnly? BeginTime { get; set; } = null;
        public DateOnly? EndTime { get; set; } = null;
        public int StatusOrder { get; set; }
        //public StatusOrder? StatusOrder { get; set; }
        public int StatusPayment { get; set; }
        //public StatusPayment? StatusPayment { get; set; }
        public IEnumerable<ChatMessage>? OrderChats { get; set; }
    }
}
