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
    public class OrderControllerTests
    {
        private ApplicationDbContext _dbContext;
        private OrderController _orderController;

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

            var items = new List<Item>
            {
                new Item { Id = 1, Name = "Item A" },
                new Item { Id = 2, Name = "Item B" },
                new Item { Id = 3, Name = "Item C" }
            };
            _dbContext.Items.AddRange(items);

            var orders = new List<Order>
            {
                new Order { Id = 1, orderInformation = "Order 1", OrderDate = DateTime.Now, CustomerId = 1, Customer = customers[0], Items = items.Take(2).ToList() },
                new Order { Id = 2, orderInformation = "Order 2", OrderDate = DateTime.Now, CustomerId = 2, Customer = customers[1], Items = items.Skip(1).Take(2).ToList() }
            };
            _dbContext.Orders.AddRange(orders);

            _dbContext.SaveChanges();

            // Create the OrderController instance with the in-memory DbContext
            _orderController = new OrderController(_dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [TestMethod]
        public async Task GetOrders_ReturnsListOfOrders()
        {
            // Act
            var result = await _orderController.GetOrders();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ActionResult<IEnumerable<Order>>));

            var actionResult = result as ActionResult<IEnumerable<Order>>;
            Assert.IsNotNull(actionResult);

            var okResult = actionResult.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var orders = okResult.Value as IEnumerable<Order>;
            Assert.IsNotNull(orders);
            Assert.AreEqual(2, orders.Count());
        }

        [TestMethod]
        public async Task GetOrders_ReturnsCorrectNumberOfOrders_WhenPageIndexAndPageSizeProvided()
        {
            // Act
            var result = await _orderController.GetOrders(pageIndex: 1, pageSize: 1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult<IEnumerable<Order>>));

            var okResult = (ActionResult<IEnumerable<Order>>)result;
            Assert.IsNotNull(okResult); // Make sure it's not null

            var orders = okResult.Value as IEnumerable<Order>;
            Assert.IsNotNull(orders); // Make sure it's not null

            Assert.AreEqual(1, orders.Count());
        }

        [TestMethod]
        public async Task GetOrders_ReturnsBadRequest_WhenInvalidPageIndexProvided()
        {
            // Act
            var result = await _orderController.GetOrders(pageIndex: -1, pageSize: 1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult<IEnumerable<Order>>));

            var actionResult = (ActionResult<IEnumerable<Order>>)result;
            Assert.IsNotNull(actionResult); // Make sure it's not null

            var badRequestResult = actionResult.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult); // Make sure it's not null

            Assert.AreEqual("Invalid page index or page size.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task GetOrders_ReturnsBadRequest_WhenInvalidPageSizeProvided()
        {
            // Act
            var result = await _orderController.GetOrders(pageIndex: 1, pageSize: 0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult<IEnumerable<Order>>));

            var actionResult = (ActionResult<IEnumerable<Order>>)result;
            var badRequestResult = actionResult.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Invalid page index or page size.", badRequestResult.Value);
        }

            [TestMethod]
            public async Task CreateOrder_ValidData_CreatesOrder()
            {
                // Arrange
                var orderInput = new orderInput
                {
                    orderInformation = "New Order",
                    CustomerId = 1,
                    Items = new List<Item>
            {
                new Item { Id = 3, Name = "Item C" }
            }
                };

                // Act
                var result = await _orderController.CreateOrder(orderInput);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(CreatedResult));

                var createdResult = result as CreatedResult;
                Assert.IsNotNull(createdResult);

                var newOrder = createdResult.Value as Order;
                Assert.IsNotNull(newOrder);

                Assert.AreEqual(StatusCodes.Status201Created, createdResult.StatusCode);
                Assert.AreEqual(orderInput.orderInformation, newOrder.orderInformation);
                Assert.AreEqual(orderInput.CustomerId, newOrder.CustomerId);
                Assert.AreEqual(orderInput.Items.Count, newOrder.Items.Count);
            }

        [TestMethod]
        public async Task CreateOrder_ReturnsBadRequest_WhenOrderInformationNotProvided()
        {
            // Arrange
            var orderInput = new orderInput
            {
                CustomerId = 1,
                Items = new List<Item>
        {
            new Item { Id = 3, Name = "Item C" }
        }
            };

            // Act
            var result = await _orderController.CreateOrder(orderInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Order information is required.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task CreateOrder_ReturnsBadRequest_WhenCustomerIdNotProvided()
        {
            // Arrange
            var orderInput = new orderInput
            {
                orderInformation = "New Order",
                Items = new List<Item>
        {
            new Item { Id = 3, Name = "Item C" }
        }
            };

            // Act
            var result = await _orderController.CreateOrder(orderInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Invalid customer ID.", badRequestResult.Value);
        }


        [TestMethod]
        public async Task UpdateOrder_ValidData_UpdatesOrder()
        {
            // Arrange
            int orderId = 1;
            var updatedOrderInput = new orderInput
            {
                orderInformation = "Updated Order",
                CustomerId = 2,
                Items = new List<Item>
                {
                    new Item { Id = 2, Name = "Item B" }
                }
            };

            // Act
            var result = await _orderController.UpdateOrder(orderId, updatedOrderInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IActionResult));

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [TestMethod]
        public async Task UpdateOrder_ReturnsBadRequest_WhenOrderInformationNotProvided()
        {
            // Arrange
            int orderId = 1;
            var updatedOrderInput = new orderInput
            {
                CustomerId = 2,
                Items = new List<Item>
                {
                    new Item { Id = 2, Name = "Item B" }
                }
            };

            // Act
            var result = await _orderController.UpdateOrder(orderId, updatedOrderInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IActionResult));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Order information is required.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task UpdateOrder_ReturnsBadRequest_WhenCustomerIdNotProvided()
        {
            // Arrange
            int orderId = 1;
            var updatedOrderInput = new orderInput
            {
                orderInformation = "Updated Order",
                Items = new List<Item>
                {
                    new Item { Id = 2, Name = "Item B" }
                }
            };

            // Act
            var result = await _orderController.UpdateOrder(orderId, updatedOrderInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IActionResult));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Invalid customer ID.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task DeleteOrder_ExistingOrderId_DeletesOrder()
        {
            // Arrange
            int orderIdToDelete = 1;

            // Act
            var result = await _orderController.DeleteOrder(orderIdToDelete);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IActionResult));

            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(StatusCodes.Status204NoContent, noContentResult.StatusCode);

            // Verify that the order has been deleted from the database
            var deletedOrder = await _dbContext.Orders.FindAsync(orderIdToDelete);
            Assert.IsNull(deletedOrder);
        }

        [TestMethod]
        public async Task DeleteOrder_ReturnsNotFound_WhenOrderNotFound()
        {
            // Arrange
            int orderIdToDelete = 999;

            // Act
            var result = await _orderController.DeleteOrder(orderIdToDelete);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IActionResult));

            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task GetOrderById_ReturnsOrder_WhenValidOrderIdProvided()
        {
            // Arrange
            int orderId = 1;

            // Act
            var result = await _orderController.GetOrderById(orderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult<Order>));

            var okResult = (ActionResult<Order>)result;
            Assert.IsNotNull(okResult);

            var order = okResult.Value as Order;
            Assert.IsNotNull(order);

            Assert.AreEqual(orderId, order.Id);
        }

        [TestMethod]
        public async Task GetOrderById_ReturnsNotFound_WhenInvalidOrderIdProvided()
        {
            // Arrange
            int orderId = 999;

            // Act
            var result = await _orderController.GetOrderById(orderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult<Order>));

            var notFoundResult = (ActionResult<Order>)result;
            Assert.IsNotNull(notFoundResult);
            Assert.IsInstanceOfType(notFoundResult.Result, typeof(NotFoundResult));

            var notFoundResultObj = notFoundResult.Result as NotFoundResult;
            Assert.IsNotNull(notFoundResultObj);
            Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResultObj.StatusCode);
        }

        [TestMethod]
        public async Task GetOrdersByCustomer_ReturnsOrders_WhenValidCustomerIdProvided()
        {
            // Arrange
            int customerId = 1;

            // Act
            var result = await _orderController.GetOrdersByCustomer(customerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult<IEnumerable<Order>>));

            var okResult = (ActionResult<IEnumerable<Order>>)result;
            Assert.IsNotNull(okResult);

            var orders = okResult.Value as IEnumerable<Order>;
            Assert.IsNotNull(orders);
            Assert.AreEqual(1, orders.Count());
            Assert.AreEqual(customerId, orders.First().CustomerId);
        }

        [TestMethod]
        public async Task GetOrdersByCustomer_ReturnsNotFound_WhenInvalidCustomerIdProvided()
        {
            // Arrange
            int customerId = 999;

            // Act
            var result = await _orderController.GetOrdersByCustomer(customerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult<IEnumerable<Order>>));

            var notFoundResult = (ActionResult<IEnumerable<Order>>)result;
            Assert.IsNotNull(notFoundResult);
            Assert.IsInstanceOfType(notFoundResult.Result, typeof(NotFoundObjectResult));

            var notFoundResultObj = notFoundResult.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResultObj);
            Assert.AreEqual("No orders found for the specified customer.", notFoundResultObj.Value);
        }

        [TestMethod]
        public async Task GetOrdersByCustomerName_ReturnsOrders_WhenValidCustomerNameProvided()
        {
            // Arrange
            string customerName = "Jane Smith";

            // Act
            var result = await _orderController.GetOrdersByCustomer(customerName);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult<IEnumerable<Order>>));

            var okResult = (ActionResult<IEnumerable<Order>>)result;
            Assert.IsNotNull(okResult);

            var orders = okResult.Value as IEnumerable<Order>;
            Assert.IsNotNull(orders);
            Assert.AreEqual(1, orders.Count());
            Assert.AreEqual(customerName, orders.First().Customer.Name);
        }

        [TestMethod]
        public async Task GetOrdersByCustomerName_ReturnsNotFound_WhenInvalidCustomerNameProvided()
        {
            // Arrange
            string customerName = "Nonexistent Customer";

            // Act
            var result = await _orderController.GetOrdersByCustomer(customerName);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult<IEnumerable<Order>>));

            var notFoundResult = (ActionResult<IEnumerable<Order>>)result;
            Assert.IsNotNull(notFoundResult);
            Assert.IsInstanceOfType(notFoundResult.Result, typeof(NotFoundObjectResult));

            var notFoundResultObj = notFoundResult.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResultObj);
            Assert.AreEqual("Customer not found.", notFoundResultObj.Value);
        }
    }
}
