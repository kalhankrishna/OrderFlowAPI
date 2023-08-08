# OrderFlowAPI
NavTech Assessment

The assessment was to create an order workflow with REST APIs where API clients will be able to:
        ○ Create orders. Order shall have order information and items.
        ○ Create customers. System shall not allow to add new customer with already
          existing customer email.
        ○ Retrieve list of orders based on specific page index and page size.

NOTE - The API can be accessed through a client like POSTMAN or through SWAGGER(default build).
       Just clone the repo and you are set to go.

NUGET Packages

API-
  Microsoft.AspNetCore.Mvc.Core
  Microsoft.EntityFrameworkCore
  Microsoft.EntityFrameworkCore.SqlServer
  Microsoft.EntityFrameworkCore.Tools
  Swashbuckle.AspNetCore
Testing-
  MSTest.TestFramework
  MSTest.TestAdapter
  Microsoft.NET.Test.Sdk
  Moq
  Microsoft.EntityFrameworkCore.InMemory
  System.Text.Json



ENTITY-RELATIONSHIP

Order Entity:

Attributes: Id (Primary Key), orderInformation, OrderDate
Relationships: Belongs to a Customer (CustomerId as Foreign Key)
Contains a collection of Items

Customer Entity:

Attributes: Id (Primary Key), Name, Email
Relationships: Can have multiple Orders

Item Entity:

Attributes: Id (Primary Key), Name
Relationships: None

Order-Customer Relationship:

Attributes: None
Represents the association between Order and Customer entities
Connects an Order to a Customer (OrderId and CustomerId as Foreign Keys)


Note: No Interfaces or hard dependency injections have been used.

APIs

Order API Endpoints:

Get Orders
Get a list of orders with optional pagination.
Endpoint: GET /api/orders
Description: Retrieve a list of orders, optionally specifying the page index and page size.

Create Order
Create a new order with order details, customer information, and items.
Endpoint: POST /api/orders
Description: Create a new order with order information, customer ID, and items.

Get Order by ID
Get order details by providing the order ID.
Endpoint: GET /api/orders/{id}
Description: Retrieve order details for a specific order by its ID.

Update Order
Update an existing order with new order details, customer information, and items.
Endpoint: PATCH /api/orders/{id}
Description: Update an existing order with new order information, customer ID, and items.

Delete Order
Delete an existing order by its ID.
Endpoint: DELETE /api/orders/{id}
Description: Delete an order by providing its ID.

Get Orders by Customer ID
Get a list of orders for a specific customer by customer ID.
Endpoint: GET /api/orders/customer/id/{customerId}
Description: Retrieve orders associated with a customer by providing their ID.

Get Orders by Customer Name
Get a list of orders for a specific customer by customer name.
Endpoint: GET /api/orders/customer/name/{customerName}
Description: Retrieve orders associated with a customer by providing their name.



Customer API endpoints:

Get Customers
Get a list of all customers.
Endpoint: GET /api/customers
Description: Retrieve a list of all customers.

Get Customer by ID
Get customer details by providing the customer ID.
Endpoint: GET /api/customers/{id}
Description: Retrieve customer details for a specific customer by their ID.

Create Customer
Create a new customer with name and email.
Endpoint: POST /api/customers
Description: Create a new customer with a name and email.

Update Customer
Update an existing customer's name and email by providing the customer ID.
Endpoint: PATCH /api/customers/{id}
Description: Update an existing customer's name and email by their ID.

Delete Customer
Delete an existing customer by their ID.
Endpoint: DELETE /api/customers/{id}
Description: Delete a customer by their ID.
