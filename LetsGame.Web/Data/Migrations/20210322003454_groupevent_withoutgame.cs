using Microsoft.EntityFrameworkCore.Migrations;

namespace LetsGame.Web.Data.Migrations
{
    public partial class groupevent_withoutgame : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupEvents_GroupGames_GameId",
                table: "GroupEvents");

            migrationBuilder.AlterColumn<long>(
                name: "GameId",
                table: "GroupEvents",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupEvents_GroupGames_GameId",
                table: "GroupEvents",
                column: "GameId",
                principalTable: "GroupGames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupEvents_GroupGames_GameId",
                table: "GroupEvents");

            migrationBuilder.AlterColumn<long>(
                name: "GameId",
                table: "GroupEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupEvents_GroupGames_GameId",
                table: "GroupEvents",
                column: "GameId",
                principalTable: "GroupGames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
