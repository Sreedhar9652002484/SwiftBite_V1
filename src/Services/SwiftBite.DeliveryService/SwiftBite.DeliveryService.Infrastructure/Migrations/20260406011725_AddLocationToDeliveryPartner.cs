using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwiftBite.DeliveryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationToDeliveryPartner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CurrentLatitude",
                table: "DeliveryPartners",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CurrentLongitude",
                table: "DeliveryPartners",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLocationUpdate",
                table: "DeliveryPartners",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentLatitude",
                table: "DeliveryPartners");

            migrationBuilder.DropColumn(
                name: "CurrentLongitude",
                table: "DeliveryPartners");

            migrationBuilder.DropColumn(
                name: "LastLocationUpdate",
                table: "DeliveryPartners");
        }
    }
}
