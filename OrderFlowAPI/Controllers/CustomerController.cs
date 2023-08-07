using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlowAPI.Data;
using OrderFlowAPI.Models;

namespace OrderFlowAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var customers = await _context.Customers.ToListAsync();
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomerById(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer(Customer customer)
        {
            if (customer == null)
            {
                return BadRequest("Customer object is null.");
            }

            if (string.IsNullOrEmpty(customer.Name))
            {
                return BadRequest("Name is required for creating a customer.");
            }

            if (string.IsNullOrEmpty(customer.Email))
            {
                return BadRequest("Email is required for creating a customer.");
            }

            if (await _context.Customers.AnyAsync(c => c.Email == customer.Email))
            {
                return Conflict("Customer with the provided email already exists.");
            }

            try
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to create the customer.");
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, Customer updatedCustomer)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            if (updatedCustomer == null)
            {
                return BadRequest("Customer object to be updated is null.");
            }

            if (string.IsNullOrEmpty(updatedCustomer.Name))
            {
                return BadRequest("Name is required for updating a customer.");
            }

            if (string.IsNullOrEmpty(updatedCustomer.Email))
            {
                return BadRequest("Email is required for updating a customer.");
            }

            if (await _context.Customers.AnyAsync(c => c.Id != id && c.Email == updatedCustomer.Email))
            {
                return Conflict("Customer with the provided email already exists.");
            }

            try
            {
                customer.Name = updatedCustomer.Name;
                customer.Email = updatedCustomer.Email;

                await _context.SaveChangesAsync();
                return Ok("Customer Updated Successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to update the customer.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            try
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                return Ok("Customer deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to delete the customer.");
            }
        }
    }
}
