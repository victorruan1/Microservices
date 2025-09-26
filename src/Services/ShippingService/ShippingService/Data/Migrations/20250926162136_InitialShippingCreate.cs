using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShippingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialShippingCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shippers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EmailId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Contact_Person = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shippers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shipper_Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Region_Id = table.Column<int>(type: "int", nullable: false),
                    Shipper_Id = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipper_Regions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipper_Regions_Regions_Region_Id",
                        column: x => x.Region_Id,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Shipper_Regions_Shippers_Shipper_Id",
                        column: x => x.Shipper_Id,
                        principalTable: "Shippers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shipping_Details",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Order_Id = table.Column<int>(type: "int", nullable: false),
                    Shipper_Id = table.Column<int>(type: "int", nullable: false),
                    Shipping_Status = table.Column<int>(type: "int", nullable: false),
                    Tracking_Number = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipping_Details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipping_Details_Shippers_Shipper_Id",
                        column: x => x.Shipper_Id,
                        principalTable: "Shippers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shipper_Regions_Region_Id_Shipper_Id",
                table: "Shipper_Regions",
                columns: new[] { "Region_Id", "Shipper_Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipper_Regions_Shipper_Id",
                table: "Shipper_Regions",
                column: "Shipper_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Shipping_Details_Shipper_Id",
                table: "Shipping_Details",
                column: "Shipper_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shipper_Regions");

            migrationBuilder.DropTable(
                name: "Shipping_Details");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "Shippers");
        }
    }
}
