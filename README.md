# CatStoreAPI
Swagger: http://catstore.runasp.net/swagger/index.html
Postman: https://documenter.getpostman.com/view/38264798/2sAYJ7gK6v
## Project structure
```
CatStoreAPI/
├── CatStoreAPI/            # Main API Project
│   ├── Auth/               # JWT Authentication Helpers
│   ├── Controllers/        # API Controllers
│   ├── DTO/                # Data Transfer Objects
│   ├── Configuration/      # AutoMapper Configurations
│   ├── Properties/         # Project Properties
│   ├── appsettings.json    # Application Configuration
│   ├── Program.cs          # Application Entry Point
│   └── CatStoreAPI.csproj
├── Core/                   # Core Interfaces and Models
│   ├── Interfaces/         # Repository and Service Interfaces
│   ├── Models/             # Domain Models
│   └── Core.csproj
├── Infrastructure/         # Data Access and Service Implementations
│   ├── DataBase/           # Database Context and Migrations
│   ├── Repositories/       # Repository Implementations
│   ├── Services/           # Service Implementations
│   └── Infrastructure.csproj
├── CatStoreAPI.sln         # Solution File
└── README.md               # Project README
```

## Key features
### 🔐 Authentication & Authorization
* User registration and login
* JWT token authentication
* Refresh token mechanism
* Change password
* Forgot password
* Reset password
* User profile editing
* Token cleanup service
### 📦 Product Management
* CRUD operations for products
* Product categorization
### 🛒 Shopping Cart
* Shopping cart creation and management
* Cart items manipulation
* Cart item quantity updates
* Cart persistence
* Cart calculations
## 💝 Wishlist
* Add/remove products to wishlist
* User-specific wishlists
### 📋 Order Management
* Order creation and processing
* Shipping address management
* Order items tracking
* Order history
### 🏷️ Category Management
* CRUD operations for categories
* Category reordering functionality
### ⭐ Review System
* Product reviews creation
* Review editing
* Rating system
### 💳 Payment Service
* Payment using stripe
### 📧 Email Service
* Email service integration
* Email templates
### 🔒 Security Features
* JWT authentication
* Token versioning
* User authorization
* Role-based access control (Admin, User)
* Refresh token mechanism
* Token blacklisting
* Token cleanup service
### 📊 Database Management
* Entity Framework Core
* SQL Server database
* Migration management
* Data relationships
* Unit of Work pattern
### 🛠️ Additional Services
* Token cleanup service
* User service
* Category reordering service
* Repository pattern implementation
* AutoMapper for DTO mappings

## Contact
* Name: Anas Al-horigy
* Email: anas.elhorigy@gmail.com
* LinkedIn: https://www.linkedin.com/in/anas-alhorigy/
