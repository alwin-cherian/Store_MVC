using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DressStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addWalletToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "wallet",
                table: "AspNetUsers",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "wallet",
                table: "AspNetUsers");
        }
    }
}
