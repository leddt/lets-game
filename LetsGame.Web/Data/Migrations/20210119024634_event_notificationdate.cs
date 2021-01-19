using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LetsGame.Web.Data.Migrations
{
    public partial class event_notificationdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartingSoonNotificationSentAtUtc",
                table: "GroupEvents",
                type: "timestamp without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartingSoonNotificationSentAtUtc",
                table: "GroupEvents");
        }
    }
}
