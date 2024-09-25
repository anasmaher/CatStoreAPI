using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "userId",
                table: "WishLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "userId",
                table: "ShoppingCarts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    firstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WishLists_userId",
                table: "WishLists",
                column: "userId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_userId",
                table: "ShoppingCarts",
                column: "userId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_Users_userId",
                table: "ShoppingCarts",
                column: "userId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WishLists_Users_userId",
                table: "WishLists",
                column: "userId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_Users_userId",
                table: "ShoppingCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_WishLists_Users_userId",
                table: "WishLists");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_WishLists_userId",
                table: "WishLists");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCarts_userId",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "userId",
                table: "WishLists");

            migrationBuilder.DropColumn(
                name: "userId",
                table: "ShoppingCarts");
        }
    }
}
