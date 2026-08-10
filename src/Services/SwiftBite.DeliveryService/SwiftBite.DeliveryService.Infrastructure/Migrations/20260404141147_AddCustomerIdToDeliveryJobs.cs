using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwiftBite.DeliveryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerIdToDeliveryJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "DeliveryJobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DeliveryJobs");
        }
    }
}
