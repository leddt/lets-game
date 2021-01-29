using Microsoft.EntityFrameworkCore.Migrations;

namespace LetsGame.Web.Data.Migrations
{
    public partial class unsubscribes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UnsubscribeEventReminder",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UnsubscribeNewEvent",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UnsubscribeVoteReminder",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnsubscribeEventReminder",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UnsubscribeNewEvent",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UnsubscribeVoteReminder",
                table: "AspNetUsers");
        }
    }
}
