using Microsoft.EntityFrameworkCore.Migrations;

namespace LetsGame.Web.Data.Migrations
{
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
    public partial class unsubscribes : Migration
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
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
