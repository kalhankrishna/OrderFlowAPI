using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrderFlowAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace OrderFlowAPI.Tests
{
    [TestClass]
    public class CustomerTests
    {
        [TestMethod]
        public void Customer_ValidProperties_ShouldPassValidation()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "John Doe",
                Email = "john@example.com"
            };

            // Act
            var validationContext = new ValidationContext(customer, null, null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(customer, validationContext, validationResults, true);

            // Assert
            Assert.IsTrue(isValid);
            Assert.AreEqual(0, validationResults.Count);
        }

        [TestMethod]
        public void Customer_InvalidEmail_ShouldFailValidation()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Jane Smith",
                Email = "invalid-email" // Invalid email format
            };

            // Act
            var validationContext = new ValidationContext(customer, null, null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(customer, validationContext, validationResults, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, validationResults.Count);

            var emailValidationResult = validationResults[0];
            Assert.AreEqual("Invalid email address format.", emailValidationResult.ErrorMessage);
            Assert.AreEqual(nameof(Customer.Email), emailValidationResult.MemberNames.First());
        }

        [TestMethod]
        public void Customer_MissingName_ShouldFailValidation()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Email = "jane@example.com"
            };

            // Act
            var validationContext = new ValidationContext(customer, null, null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(customer, validationContext, validationResults, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, validationResults.Count);

            var nameValidationResult = validationResults[0];
            Assert.AreEqual("The Name field is required.", nameValidationResult.ErrorMessage);
            Assert.AreEqual(nameof(Customer.Name), nameValidationResult.MemberNames.First());
        }

        [TestMethod]
        public void Customer_MissingEmail_ShouldFailValidation()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Jane Smith"
            };

            // Act
            var validationContext = new ValidationContext(customer, null, null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(customer, validationContext, validationResults, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, validationResults.Count);

            var emailValidationResult = validationResults[0];
            Assert.AreEqual("The Email field is required.", emailValidationResult.ErrorMessage);
            Assert.AreEqual(nameof(Customer.Email), emailValidationResult.MemberNames.First());
        }
    }
}
