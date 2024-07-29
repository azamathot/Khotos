using ChatAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Chats;

namespace ChatAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<List<ChatMessage>>> GetMessagesByOrderId(int orderId)
        {
            var chatList = await _chatService.GetMessagesByOrderAsync(orderId);
            return Ok(chatList);
        }

        //[HttpPut]
        //public async Task<IActionResult> PutMessage(ChatMessage chatMessage)
        //{
        //    await _chatService.UpdateChatMessageAsync(chatMessage);
        //    return Ok();
        //}

        //[HttpPost]
        //public async Task<ActionResult<ChatMessage>> PostChatMessage(ChatMessage chatMessage)
        //{
        //    await _chatService.AddMessageAsync(chatMessage);

        //    return CreatedAtAction("GetMessageById", new { id = chatMessage.Id }, chatMessage);
        //}

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteChatMessage(int id)
        //{
        //    _chatService.DeleteMessageAsync(id);
        //    return NoContent();
        //}

        [HttpGet("getCountNewMessageByOrder/{orderId}")]
        public async Task<ActionResult<int>> GetCountNewMessageByOrder(int orderId)
        {
            return Ok(await _chatService.GetCountNewMessageByOrder(orderId));
        }
    }
}
