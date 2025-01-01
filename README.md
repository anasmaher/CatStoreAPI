# CatStoreAPI

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
* 🔐 Authentication & Authorization
* User registration and login
* JWT token authentication
* Refresh token mechanism
* Password management:
* Change password
* Forgot password
* Reset password
* User profile editing
* Token cleanup service
* 📦 Product Management
* CRUD operations for products
* Product categorization
* Product images handling
* Product-category relationships
* Product updates and modifications
* Product listing and details
* 🛒 Shopping Cart
* Shopping cart creation and management
* Cart items manipulation
* Cart item quantity updates
* Cart persistence
* Shopping cart items tracking
* Cart calculations
* 💝 Wishlist
* Add/remove products to wishlist
* Wishlist management
* User-specific wishlists
* Wishlist items tracking
* 📋 Order Management
* Order creation and processing
* Shipping address management
* Order items tracking
* Order status handling
* Order history
* 🏷️ Category Management
* CRUD operations for categories
* Category reordering functionality
* Category-product relationships
* Category hierarchy management
* ⭐ Review System
* Product reviews creation
* Review editing
* Review management
* Rating system
* Review-product association
* 💳 Payment Service
* Payment processing
* Payment service integration
* Payment handling
* 📧 Email Service
* Email notifications
* Email service integration
* Email templates
* 🔒 Security Features
* JWT authentication
* Token versioning
* Secure password handling
* User authorization
* 📊 Database Management
* Entity Framework Core
* SQL Server database
* Migration management
* Data relationships
* Unit of Work pattern
* 🛠️ Additional Services
* Token cleanup service
* User service
* Category reordering service
* Repository pattern implementation
AutoMapper for DTO mappings
