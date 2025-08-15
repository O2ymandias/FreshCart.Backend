# FreshCart Backend

A modern e-commerce backend platform built with ASP.NET Core using Clean Architecture principles. This RESTful API provides comprehensive functionality for managing products, orders, payments, and user authentication in an e-commerce environment.

> **Related Project**: [FreshCart Frontend](https://github.com/O2ymandias/FreshCart.Frontend) - Angular frontend application

## 🏗️ Architecture Overview

FreshCart.Backend follows Clean Architecture principles, ensuring separation of concerns and maintainability:

- **API Layer** (`ECommerce.APIs`) - Controllers, middleware, filters, error and response models, localization resources, and api configuration
- **Application Layer** (`ECommerce.Application`) - Service implementations (Auth, Cart, Checkout, Product, etc.) and AutoMapper profiles
- **Domain Layer** (`ECommerce.Core`) - Entities, DTOs, interfaces, specifications, enums, constants and shared classes
- **Infrastructure Layer** (`ECommerce.Infrastructure`) - Database context, migrations, repositories

### Design Patterns

The project implements key design patterns for maintainable and scalable code:

- **Specification Pattern** - Encapsulates complex query logic and business rules
- **Repository Pattern** - Abstracts data access logic with consistent interfaces
- **Unit of Work Pattern** - Manages transactions and coordinates repository operations
- **Factory Pattern** - Creates objects without exposing instantiation logic

## ⚙️ Configuration

### 1. Clone the Repository

```bash
git clone https://github.com/O2ymandias/FreshCart.Backend.git
cd FreshCart.Backend
```

### 2. Configure Application Settings

Update `ECommerce.APIs/appsettings.Development.json` with your local settings:

```json
{
	"ConnectionStrings": {
		"Default": "",
		"Redis": ""
	},
	"AdminOptions": {
		"Email": "",
		"Password": ""
	},
	"JwtOptions": {
		"SecurityKey": ""
	},
	"EmailOptions": {
		"SenderEmail": "",
		"Password": ""
	},
	"StripeOptions": {
		"SecretKey": "",
		"PublishableKey": "",
		"WebhookSecret": ""
	}
}
```

## 📚 API Documentation

Once the application is running, you can access:

- **Swagger UI**: `BaseUrl/swagger`
- **API Endpoints**: `BaseUrl/api/`
- **Postman Collection**: Import the `FreshCart.postman-apis-collection.json`
  
**Happy Coding with FreshCart Backend! 🚀**
