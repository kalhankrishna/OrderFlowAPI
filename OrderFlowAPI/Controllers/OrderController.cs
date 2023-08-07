using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlowAPI.Data;
using OrderFlowAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders(int? pageIndex = null, int? pageSize = null)
        {
            // Ensure non-negative page index and page size if provided
            if (pageIndex.HasValue && pageIndex <= 0 || pageSize.HasValue && pageSize <= 0)
            {
                return BadRequest("Invalid page index or page size.");
            }

            // Calculate the number of orders to skip based on the page index and page size
            int ordersToSkip = (pageIndex.GetValueOrDefault(1) - 1) * pageSize.GetValueOrDefault(10);

            // Retrieve the paginated list of orders
            var orders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .Skip(ordersToSkip)
                .Take(pageSize.GetValueOrDefault(10))
                .ToListAsync();

            return Ok(orders);
        }

        // POST api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder(orderInput order)
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
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == order.CustomerId);
            if (customer == null)
            {
                return NotFound("Customer with the provided ID not found.");
            }

            try
            {
                Order newOrder = new Order();
                newOrder.orderInformation = order.orderInformation;
                newOrder.CustomerId = order.CustomerId;
                newOrder.Customer = customer;
                newOrder.OrderDate = DateTime.Now;
                newOrder.Items = order.Items;

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetOrderById), new { id = newOrder.Id }, newOrder);
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to create the order.");
            }
        }

        // GET api/orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            var order = await _context.Orders
                        .Include(o => o.Customer)
                        .Include(o => o.Items)
                        .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // PATCH api/orders/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, orderInput updatedOrder)
        {
            var order = await _context.Orders.FindAsync(id);
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
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == updatedOrder.CustomerId);
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
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            try
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to delete the order.");
            }
        }

        // GET api/orders/customer/id/{customerId}
        [HttpGet("customer/id/{customerId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByCustomer(int customerId)
        {
            var orders = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .ToListAsync();

            if (orders.Count == 0)
            {
                return NotFound("No orders found for the specified customer.");
            }

            return Ok(orders);
        }

        // GET api/orders/customer/name/{customerName}
        [HttpGet("customer/name/{customerName}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByCustomer(string customerName)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Name == customerName);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            var orders = await _context.Orders
                .Where(o => o.CustomerId == customer.Id)
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .ToListAsync();

            if (orders.Count == 0)
            {
                return NotFound("No orders found for the specified customer.");
            }

            return Ok(orders);
        }
    }
}
