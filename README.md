Containerized E-Commerce Microservices Backend Midterm Mini Project (deliverable a) for the CSCI 6844 | Programming for the Internet course.

All services and processes are set according to the project requirements given by the course's professor.

The application consists of a containerized distributed backend system for a simplified e-commerce platform using ASP.NET Core, EF Core, Docker, and Docker Compose.

4 Microservices are included:
- CustomerService: Manages customer records (name, email)
- ProductService: Manages product catalog (name, description, price, stock)
- ShippingService: Auto-creates a shipment record when an order is placed
- OrderService: Creates and tracks orders; orchestrates validation and shipment

No gitignore, given that it's an educational project.

Since the last deliverable, the microservices architecture was strengthened by:
•	Implementation of asynchronous messaging using RabbitMQ
•	Implementation of an API Gateway for centralized access (Ocelot)
•	use of Data Transfer Object (DTO) based design to improve data handling and to reduce coupling
•	ProductService stock started to be decreased by the OrderService
•	Implementation of one aggregated endpoint (order + product + customer data)
•	Update of the docker-compose.yml
•	Implementation of Order Deletion

