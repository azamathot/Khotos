using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ordering.Data;
using SharedModels.Orders;
using System.Security.Claims;

namespace Ordering.Controllers
{
    [Authorize]
    [Route("ordering")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersDbContext _db;
        private string _userId;

        public OrdersController(OrdersDbContext context)
        {
            _db = context;
        }


        [Authorize]
        [HttpGet("getcustomerorders")]
        public ActionResult<IEnumerable<OrderView>> GetCustomerOrdersAsync()
        {
            _userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (_db.Orders == null)
            {
                return NotFound();
            }
            return _db.Orders
                .Where(x => x.UserId == _userId)
                .GroupJoin(_db.ChatMessages.DefaultIfEmpty(),
                o => o.Id,
                c => c.OrderId,
                (o, chatList) => new OrderView()
                {
                    CountNewMessage = chatList.Count(x => x.UserId != _userId && !x.IsReaded),
                    Order = o,
                    LastChange = chatList.Any() ? chatList.Max(x => x.SendTime) : o.Created
                }).ToList();
        }

        [Authorize(Roles = "admin")]
        [HttpGet("getallorders")]
        public ActionResult<IEnumerable<OrderView>> GetAllOrdersAsync()
        {
            _userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (_db.Orders == null)
            {
                return NotFound();
            }
            return _db.Orders
                .GroupJoin(_db.ChatMessages,
                o => o.Id,
                c => c.OrderId,
                (o, chatList) => new OrderView()
                {
                    CountNewMessage = chatList.Count(x => x.UserId != _userId && !x.IsReaded),
                    Order = o,
                    LastChange = chatList.Any() ? chatList.Max(x => x.SendTime) : o.Created
                }).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _db.Orders
                //.Include("OrderChats")
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            _db.Entry(order).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPut("UpdateOrderPaymentStatus/{id}/{paymentStatus}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, int paymentStatus)
        {
            var order = _db.Orders.FirstOrDefault(x => x.Id == id);
            if (order == null)
                return NotFound();
            order.StatusPayment = paymentStatus;
            await _db.SaveChangesAsync();
            return Ok();
        }

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            if (_db.Orders == null)
            {
                return Problem("Entity set 'OrdersDbContext.Orders'  is null.");
            }
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            if (_db.Orders == null)
            {
                return NotFound();
            }
            var order = await _db.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return (_db.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
