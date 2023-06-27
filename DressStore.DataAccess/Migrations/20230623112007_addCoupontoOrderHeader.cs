using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DressStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addCoupontoOrderHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CouponDiscount",
                table: "OrderHeader",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NewOrderTotal",
                table: "OrderHeader",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponDiscount",
                table: "OrderHeader");

            migrationBuilder.DropColumn(
                name: "NewOrderTotal",
                table: "OrderHeader");
        }
    }
}
