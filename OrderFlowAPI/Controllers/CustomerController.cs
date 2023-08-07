using Microsoft.AspNetCore.Mvc;
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

        // GET api/customer
        [HttpGet]
        public ActionResult<IEnumerable<Customer>> GetCustomers()
        {
            return _context.Customers.ToList();
        }

        // GET api/customer/{id}
        [HttpGet("{id}")]
        public ActionResult<Customer> GetCustomerById(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }
            return customer;
        }

        // POST api/customer
        [HttpPost]
        public ActionResult<Customer> CreateCustomer(Customer customer)
        {
            if (customer == null)
            {
                return BadRequest("Customer object is null.");
            }

            // Check if Name is provided and not empty
            if (string.IsNullOrEmpty(customer.Name))
            {
                return BadRequest("Name is required for creating a customer.");
            }

            // Check if Email is provided and not empty
            if (string.IsNullOrEmpty(customer.Email))
            {
                return BadRequest("Email is required for creating a customer.");
            }

            // Check if a customer with the provided email already exists
            if (_context.Customers.Any(c => c.Email == customer.Email))
            {
                return Conflict("Customer with the provided email already exists.");
            }

            try
            {
                _context.Customers.Add(customer);
                _context.SaveChanges();
                return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to create the customer.");
            }
        }

        // PATCH api/customer/{id}
        [HttpPatch("{id}")]
        public IActionResult UpdateCustomer(int id, Customer updatedCustomer)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }

            // Check if Customer is provided
            if (updatedCustomer == null)
            {
                return BadRequest("Customer object to be updated is null.");
            }

            // Check if Name is provided and not empty
            if (string.IsNullOrEmpty(customer.Name))
            {
                return BadRequest("Email is required for creating a customer.");
            }

            // Check if Email is provided and not empty
            if (string.IsNullOrEmpty(updatedCustomer.Email))
            {
                return BadRequest("Email is required for updating a customer.");
            }

            // Check if the provided email is unique and not used by other customers
            if (_context.Customers.Any(c => c.Id != id && c.Email == updatedCustomer.Email))
            {
                return Conflict("Customer with the provided email already exists.");
            }

            try
            {
                customer.Name = updatedCustomer.Name;
                customer.Email = updatedCustomer.Email;

                _context.SaveChanges();
                return Ok("Customer Updated Successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to update the customer.");
            }
        }

        // DELETE api/customer/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteCustomer(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }

            try
            {
                _context.Customers.Remove(customer);
                _context.SaveChanges();
                return Ok("Customer deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to delete the customer.");
            }
        }
    }
}