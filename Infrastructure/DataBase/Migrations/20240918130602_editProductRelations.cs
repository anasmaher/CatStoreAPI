using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editProductRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_Products_ProductId",
                table: "ShoppingCarts");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCarts_ProductId",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "ShoppingCartId",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ShoppingCarts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShoppingCartId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_ProductId",
                table: "ShoppingCarts",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_Products_ProductId",
                table: "ShoppingCarts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
