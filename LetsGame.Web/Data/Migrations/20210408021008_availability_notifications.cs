using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LetsGame.Web.Data.Migrations
{
    public partial class availability_notifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AvailabilityNotificationSentAtUtc",
                table: "Memberships",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UnsubscribeMemberAvailable",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "UnsubscribeMemberAvailablePush",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityNotificationSentAtUtc",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "UnsubscribeMemberAvailable",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UnsubscribeMemberAvailablePush",
                table: "AspNetUsers");
        }
    }
}
