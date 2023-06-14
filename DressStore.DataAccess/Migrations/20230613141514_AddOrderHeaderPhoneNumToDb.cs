using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DressStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderHeaderPhoneNumToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "phoneNumber",
                table: "OrderHeader",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "phoneNumber",
                table: "OrderHeader");
        }
    }
}
