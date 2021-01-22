using Microsoft.EntityFrameworkCore.Migrations;

namespace LetsGame.Web.Data.Migrations
{
    public partial class group_sharingkey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SharingKey",
                table: "Groups",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SharingKey",
                table: "Groups");
        }
    }
}
