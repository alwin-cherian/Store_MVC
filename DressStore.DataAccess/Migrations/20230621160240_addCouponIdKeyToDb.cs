using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DressStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addCouponIdKeyToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsValid",
                table: "Coupons",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsValid",
                table: "Coupons");
        }

    }
}
