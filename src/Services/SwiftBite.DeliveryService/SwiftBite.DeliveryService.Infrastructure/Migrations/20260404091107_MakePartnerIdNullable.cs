using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwiftBite.DeliveryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakePartnerIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryJobs_DeliveryPartners_PartnerId",
                table: "DeliveryJobs");

            migrationBuilder.AlterColumn<Guid>(
                name: "PartnerId",
                table: "DeliveryJobs",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryJobs_DeliveryPartners_PartnerId",
                table: "DeliveryJobs",
                column: "PartnerId",
                principalTable: "DeliveryPartners",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryJobs_DeliveryPartners_PartnerId",
                table: "DeliveryJobs");

            migrationBuilder.AlterColumn<Guid>(
                name: "PartnerId",
                table: "DeliveryJobs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryJobs_DeliveryPartners_PartnerId",
                table: "DeliveryJobs",
                column: "PartnerId",
                principalTable: "DeliveryPartners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
