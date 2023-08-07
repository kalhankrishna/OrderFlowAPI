using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrderFlowAPI.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrderFlowAPI.Tests
{
    [TestClass]
    public class OrderTests
    {
        [TestMethod]
        public void Order_ValidProperties_ShouldPassValidation()
        {
            // Arrange
            var customer = new Customer { Id = 1, Name = "John Doe", Email = "john@example.com" };
            var items = new List<Item> { new Item { Id = 1, Name = "Item A" } };
            var order = new Order
            {
                Id = 1,
                orderInformation = "Order 1",
                OrderDate = DateTime.Now,
                CustomerId = customer.Id,
                Customer = customer,
                Items = items
            };

            // Act
            var validationContext = new ValidationContext(order, null, null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Assert
            Assert.IsTrue(isValid);
            Assert.AreEqual(0, validationResults.Count);
        }

        [TestMethod]
        public void Order_MissingOrderInformation_ShouldFailValidation()
        {
            // Arrange
            var customer = new Customer { Id = 1, Name = "John Doe", Email = "john@example.com" };
            var items = new List<Item> { new Item { Id = 1, Name = "Item A" } };
            var order = new Order
            {
                Id = 1,
                OrderDate = DateTime.Now,
                CustomerId = customer.Id,
                Customer = customer,
                Items = items
            };

            // Act
            var validationContext = new ValidationContext(order, null, null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, validationResults.Count);

            var orderInfoValidationResult = validationResults[0];
            Assert.AreEqual("The orderInformation field is required.", orderInfoValidationResult.ErrorMessage);
            Assert.AreEqual(nameof(Order.orderInformation), orderInfoValidationResult.MemberNames.First());
        }

        [TestMethod]
        public void Order_MissingCustomerId_ShouldFailValidation()
        {
            // Arrange
            var items = new List<Item> { new Item { Id = 1, Name = "Item A" } };
            var order = new Order
            {
                Id = 1,
                orderInformation = "Order 1",
                OrderDate = DateTime.Now,
                Items = items
            };

            // Act
            var validationContext = new ValidationContext(order, null, null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, validationResults.Count);

            var customerIdValidationResult = validationResults[0];
            Assert.AreEqual("The CustomerId field is required.", customerIdValidationResult.ErrorMessage);
            Assert.AreEqual(nameof(Order.CustomerId), customerIdValidationResult.MemberNames.First());
        }

        [TestMethod]
        public void Order_MissingItems_ShouldFailValidation()
        {
            // Arrange
            var customer = new Customer { Id = 1, Name = "John Doe", Email = "john@example.com" };
            var order = new Order
            {
                Id = 1,
                orderInformation = "Order 1",
                OrderDate = DateTime.Now,
                CustomerId = customer.Id,
                Customer = customer
            };

            // Act
            var validationContext = new ValidationContext(order, null, null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, validationResults.Count);

            var itemsValidationResult = validationResults[0];
            Assert.AreEqual("The Items field is required.", itemsValidationResult.ErrorMessage);
            Assert.AreEqual(nameof(Order.Items), itemsValidationResult.MemberNames.First());
        }
    }
}
