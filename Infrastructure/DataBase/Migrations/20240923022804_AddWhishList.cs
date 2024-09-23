using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWhishList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WishLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WishlistProducts",
                columns: table => new
                {
                    ProductsId = table.Column<int>(type: "int", nullable: false),
                    WhishListsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishlistProducts", x => new { x.ProductsId, x.WhishListsId });
                    table.ForeignKey(
                        name: "FK_WishlistProducts_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WishlistProducts_WishLists_WhishListsId",
                        column: x => x.WhishListsId,
                        principalTable: "WishLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WishlistProducts_WhishListsId",
                table: "WishlistProducts",
                column: "WhishListsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WishlistProducts");

            migrationBuilder.DropTable(
                name: "WishLists");
        }
    }
}
