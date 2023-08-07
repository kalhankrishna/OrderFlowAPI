using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OrderFlowAPI.Controllers;
using OrderFlowAPI.Data;
using OrderFlowAPI.Models;

namespace OrderFlowAPI.Tests
{
    [TestClass]
    public class CustomerControllerTests
    {
        private ApplicationDbContext _dbContext;
        private CustomerController _customerController;

        [TestInitialize]
        public void Initialize()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _dbContext.Database.EnsureCreated();

            // Seed some test data
            var customers = new List<Customer>
            {
                new Customer { Id = 1, Name = "John Doe", Email = "john@example.com" },
                new Customer { Id = 2, Name = "Jane Smith", Email = "jane@example.com" }
            };
            _dbContext.Customers.AddRange(customers);
            _dbContext.SaveChanges();

            // Create the CustomerController instance with the in-memory DbContext
            _customerController = new CustomerController(_dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [TestMethod]
        public async Task GetCustomers_ReturnsListOfCustomers()
        {
            // Act
            var result = await _customerController.GetCustomers();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult<IEnumerable<Customer>>));

            var okResult = (ActionResult<IEnumerable<Customer>>)result;
            Assert.IsNotNull(okResult);

            var customers = okResult.Value as IEnumerable<Customer>;
            Assert.IsNotNull(customers);

            Assert.AreEqual(2, customers.Count());
        }

        [TestMethod]
        public async Task GetCustomerById_ReturnsCustomer_WhenValidCustomerIdProvided()
        {
            // Arrange
            int customerId = 1;

            // Act
            var result = await _customerController.GetCustomerById(customerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult<Customer>));

            var okResult = (ActionResult<Customer>)result;
            Assert.IsNotNull(okResult);

            var customer = okResult.Value as Customer;
            Assert.IsNotNull(customer);

            Assert.AreEqual(customerId, customer.Id);
        }

        [TestMethod]
        public async Task GetCustomerById_ReturnsNotFound_WhenInvalidCustomerIdProvided()
        {
            // Arrange
            int customerId = 999;

            // Act
            var result = await _customerController.GetCustomerById(customerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult<Customer>));

            var notFoundResult = (ActionResult<Customer>)result;
            Assert.IsNotNull(notFoundResult);
            Assert.IsInstanceOfType(notFoundResult.Result, typeof(NotFoundResult));

            var notFoundResultObj = notFoundResult.Result as NotFoundResult;
            Assert.IsNotNull(notFoundResultObj);
            Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResultObj.StatusCode);
        }

        [TestMethod]
        public async Task CreateCustomer_ValidData_CreatesCustomer()
        {
            // Arrange
            var customerInput = new Customer
            {
                Name = "New Customer",
                Email = "new@example.com"
            };

            // Act
            var result = await _customerController.CreateCustomer(customerInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(CreatedResult)); // Check for CreatedResult type

            var createdResult = result as CreatedResult;
            Assert.IsNotNull(createdResult);

            var newCustomer = createdResult.Value as Customer;
            Assert.IsNotNull(newCustomer);

            Assert.AreEqual(StatusCodes.Status201Created, createdResult.StatusCode); // Check status code

            // Verify that the customer has been added to the database
            var addedCustomer = await _dbContext.Customers.FindAsync(newCustomer.Id);
            Assert.IsNotNull(addedCustomer);
            Assert.AreEqual(newCustomer.Id, addedCustomer.Id);
        }

        [TestMethod]
        public async Task CreateCustomer_ReturnsBadRequest_WhenNameNotProvided()
        {
            // Arrange
            var customerInput = new Customer
            {
                Email = "new@example.com"
            };

            // Act
            var result = await _customerController.CreateCustomer(customerInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Name is required for creating a customer.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task CreateCustomer_ReturnsBadRequest_WhenEmailNotProvided()
        {
            // Arrange
            var customerInput = new Customer
            {
                Name = "New Customer"
            };

            // Act
            var result = await _customerController.CreateCustomer(customerInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Email is required for creating a customer.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task CreateCustomer_ReturnsConflict_WhenCustomerWithEmailAlreadyExists()
        {
            // Arrange
            var existingCustomer = new Customer
            {
                Name = "John Doe",
                Email = "john@example.com"
            };

            // Act
            var result = await _customerController.CreateCustomer(existingCustomer);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));

            var conflictResult = result as ConflictObjectResult;
            Assert.IsNotNull(conflictResult);
            Assert.AreEqual("Customer with the provided email already exists.", conflictResult.Value);
        }

        [TestMethod]
        public async Task UpdateCustomer_ValidData_UpdatesCustomer()
        {
            // Arrange
            int customerId = 1;
            var updatedCustomerInput = new Customer
            {
                Name = "Updated Customer",
                Email = "updated@example.com"
            };

            // Act
            var result = await _customerController.UpdateCustomer(customerId, updatedCustomerInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult));

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            // Verify that the customer has been updated in the database
            var updatedCustomer = await _dbContext.Customers.FindAsync(customerId);
            Assert.IsNotNull(updatedCustomer);
            Assert.AreEqual(updatedCustomerInput.Name, updatedCustomer.Name);
            Assert.AreEqual(updatedCustomerInput.Email, updatedCustomer.Email);
        }

        [TestMethod]
        public async Task UpdateCustomer_ReturnsBadRequest_WhenCustomerNotFound()
        {
            // Arrange
            int customerId = 999;
            var updatedCustomerInput = new Customer
            {
                Name = "Updated Customer",
                Email = "updated@example.com"
            };

            // Act
            var result = await _customerController.UpdateCustomer(customerId, updatedCustomerInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));

            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task UpdateCustomer_ReturnsBadRequest_WhenNameNotProvided()
        {
            // Arrange
            int customerId = 1;
            var updatedCustomerInput = new Customer
            {
                Email = "updated@example.com"
            };

            // Act
            var result = await _customerController.UpdateCustomer(customerId, updatedCustomerInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Email is required for creating a customer.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task UpdateCustomer_ReturnsBadRequest_WhenEmailNotProvided()
        {
            // Arrange
            int customerId = 1;
            var updatedCustomerInput = new Customer
            {
                Name = "Updated Customer"
            };

            // Act
            var result = await _customerController.UpdateCustomer(customerId, updatedCustomerInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Email is required for updating a customer.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task UpdateCustomer_ReturnsConflict_WhenCustomerWithEmailAlreadyExists()
        {
            // Arrange
            int customerId = 1;
            var existingCustomer = new Customer
            {
                Name = "Jane Smith",
                Email = "jane@example.com"
            };

            // Act
            var result = await _customerController.UpdateCustomer(customerId, existingCustomer);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));

            var conflictResult = result as ConflictObjectResult;
            Assert.IsNotNull(conflictResult);
            Assert.AreEqual("Customer with the provided email already exists.", conflictResult.Value);
        }

        [TestMethod]
        public async Task DeleteCustomer_ExistingCustomerId_DeletesCustomer()
        {
            // Arrange
            int customerIdToDelete = 1;

            // Act
            var result = await _customerController.DeleteCustomer(customerIdToDelete);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult));

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            // Verify that the customer has been deleted from the database
            var deletedCustomer = await _dbContext.Customers.FindAsync(customerIdToDelete);
            Assert.IsNull(deletedCustomer);
        }

        [TestMethod]
        public async Task DeleteCustomer_ReturnsNotFound_WhenCustomerNotFound()
        {
            // Arrange
            int customerIdToDelete = 999;

            // Act
            var result = await _customerController.DeleteCustomer(customerIdToDelete);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));

            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }
    }
}
