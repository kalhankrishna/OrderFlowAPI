using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlowAPI.Data;
using OrderFlowAPI.Models;

namespace OrderFlowAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/orders
        [HttpGet]
        public ActionResult<IEnumerable<Order>> GetOrders(int? pageIndex = null, int? pageSize = null)
        {
            // Ensure non-negative page index and page size if provided
            if (pageIndex.HasValue && pageIndex <= 0 || pageSize.HasValue && pageSize <= 0)
            {
                return BadRequest("Invalid page index or page size.");
            }

            // Calculate the number of orders to skip based on the page index and page size
            int ordersToSkip = (pageIndex.GetValueOrDefault(1) - 1) * pageSize.GetValueOrDefault(10);

            // Retrieve the paginated list of orders
            var orders = _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .Skip(ordersToSkip)
                .Take(pageSize.GetValueOrDefault(10))
                .ToList();

            return orders;
        }


        // POST api/orders
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(orderInput order)
        {
            if (order == null)
            {
                return BadRequest("Order object is null.");
            }

            // Check if orderInformation is provided
            if (string.IsNullOrEmpty(order.orderInformation))
            {
                return BadRequest("Order information is required.");
            }

            // Check if CustomerId is valid (greater than zero)
            if (order.CustomerId <= 0)
            {
                return BadRequest("Invalid customer ID.");
            }

            // Check if Items collection is provided and not empty
            if (order.Items == null || order.Items.Count == 0)
            {
                return BadRequest("At least one item is required for the order.");
            }

            // Perform validations for items in the order
            foreach (var item in order.Items)
            {
                if (string.IsNullOrEmpty(item.Name))
                {
                    return BadRequest("Item name is required.");
                }
            }

            // Check if Customer with provided CustomerId exists in the database
            var customer = _context.Customers.FirstOrDefault(c => c.Id == order.CustomerId);
            if (customer == null)
            {
                return NotFound("Customer with the provided ID not found.");
            }

            try
            {
                Order newOrder = new Order();
                newOrder.orderInformation = order.orderInformation;
                newOrder.CustomerId = order.CustomerId;
                newOrder.Customer = await _context.Customers.Where(x => x.Id == order.CustomerId).FirstOrDefaultAsync();
                newOrder.OrderDate = DateTime.Now;
                newOrder.Items = order.Items;

                _context.Orders.Add(newOrder);
                _context.SaveChanges();
                return CreatedAtAction(nameof(GetOrderById), new { id = newOrder.Id }, newOrder);
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to create the order.");
            }
        }

        // GET api/orders/{id}
        [HttpGet("{id}")]
        public ActionResult<Order> GetOrderById(int id)
        {
            var order = _context.Orders
                        .Include(o => o.Customer)
                        .Include(o => o.Items)
                        .FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // PATCH api/orders/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, orderInput updatedOrder)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
            {
                return NotFound("Order not found.");
            }

            // Perform validations here (e.g., check if required fields are provided)
            if (updatedOrder == null)
            {
                return BadRequest("Updated order object is null.");
            }

            // Check if orderInformation is provided
            if (string.IsNullOrEmpty(updatedOrder.orderInformation))
            {
                return BadRequest("Order information is required.");
            }

            // Check if CustomerId is valid (greater than zero)
            if (updatedOrder.CustomerId <= 0)
            {
                return BadRequest("Invalid customer ID.");
            }

            // Check if Items collection is provided and not empty
            if (updatedOrder.Items == null || updatedOrder.Items.Count == 0)
            {
                return BadRequest("At least one item is required for the order.");
            }

            // Perform validations for items in the order
            foreach (var item in updatedOrder.Items)
            {
                if (string.IsNullOrEmpty(item.Name))
                {
                    return BadRequest("Item name is required.");
                }
            }

            // Check if Customer with provided CustomerId exists in the database
            var customer = _context.Customers.FirstOrDefault(c => c.Id == updatedOrder.CustomerId);
            if (customer == null)
            {
                return NotFound("Customer with the provided ID not found.");
            }

            try
            {
                // Update the existing order with the new data
                order.orderInformation = updatedOrder.orderInformation;
                order.CustomerId = updatedOrder.CustomerId;
                order.Customer = customer;
                order.OrderDate = DateTime.Now;
                order.Items = updatedOrder.Items;

                await _context.SaveChangesAsync();
                return Ok("Order updated successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to update the order.");
            }
        }

        // DELETE api/orders/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            try
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to delete the order.");
            }
        }

        // GET api/orders/customer/{customerId}
        [HttpGet("customer/id/{customerId}")]
        public ActionResult<IEnumerable<Order>> GetOrdersByCustomer(int customerId)
        {
            var orders = _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .ToList();

            if (orders.Count == 0)
            {
                return NotFound("No orders found for the specified customer.");
            }

            return orders;
        }

        // GET api/orders/customer/name/{customerName}
        [HttpGet("customer/name/{customerName}")]
        public ActionResult<IEnumerable<Order>> GetOrdersByCustomer(string customerName)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Name == customerName);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            var orders = _context.Orders
                .Where(o => o.CustomerId == customer.Id)
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .ToList();

            if (orders.Count == 0)
            {
                return NotFound("No orders found for the specified customer.");
            }

            return orders;
        }
    }
}