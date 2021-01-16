using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LetsGame.Web.Data.Migrations
{
    public partial class events : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    GameId = table.Column<long>(type: "bigint", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ChosenDateAndTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupEvents_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GroupEvents_GroupGames_GameId",
                        column: x => x.GameId,
                        principalTable: "GroupGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupEvents_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GroupEventSlots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<long>(type: "bigint", nullable: false),
                    ProposedDateAndTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupEventSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupEventSlots_GroupEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "GroupEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupEventSlotVotes",
                columns: table => new
                {
                    SlotId = table.Column<long>(type: "bigint", nullable: false),
                    VoterId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VotedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupEventSlotVotes", x => new { x.SlotId, x.VoterId });
                    table.ForeignKey(
                        name: "FK_GroupEventSlotVotes_AspNetUsers_VoterId",
                        column: x => x.VoterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupEventSlotVotes_GroupEventSlots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "GroupEventSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupEvents_CreatorId",
                table: "GroupEvents",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupEvents_GameId",
                table: "GroupEvents",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupEvents_GroupId",
                table: "GroupEvents",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupEventSlots_EventId",
                table: "GroupEventSlots",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupEventSlotVotes_VoterId",
                table: "GroupEventSlotVotes",
                column: "VoterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupEventSlotVotes");

            migrationBuilder.DropTable(
                name: "GroupEventSlots");

            migrationBuilder.DropTable(
                name: "GroupEvents");
        }
    }
}
