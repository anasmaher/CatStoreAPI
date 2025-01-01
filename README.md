# CatStoreAPI

## Project structure

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


