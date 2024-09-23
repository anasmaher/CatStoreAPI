using CatStoreAPI.Configuration;
using Core.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Helpers;

namespace CatStoreAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseLazyLoadingProxies().UseSqlServer(connectionString));

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
            builder.Services.AddScoped<IWishListRepository, WishListRepository>();
            builder.Services.AddScoped<ReorderCategoriesHelper>();

            builder.Services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            builder.Services.AddAutoMapper(typeof(MappingConfig));

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
