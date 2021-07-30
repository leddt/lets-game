using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LetsGame.Web.Data.Migrations
{
    public partial class notifications_allvotes_slotpicked : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AllVotesInNotificationSentAtUtc",
                table: "GroupEvents",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UnsubscribeAllVotesIn",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UnsubscribeAllVotesInPush",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UnsubscribeSlotPicked",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UnsubscribeSlotPickedPush",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllVotesInNotificationSentAtUtc",
                table: "GroupEvents");

            migrationBuilder.DropColumn(
                name: "UnsubscribeAllVotesIn",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UnsubscribeAllVotesInPush",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UnsubscribeSlotPicked",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UnsubscribeSlotPickedPush",
                table: "AspNetUsers");
        }
    }
}
