using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LetsGame.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class private_schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "private");

            migrationBuilder.RenameTable(
                name: "UserPushSubscription",
                newName: "UserPushSubscription",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "Memberships",
                newName: "Memberships",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "Groups",
                newName: "Groups",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "GroupInvites",
                newName: "GroupInvites",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "GroupGames",
                newName: "GroupGames",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "GroupEventSlotVotes",
                newName: "GroupEventSlotVotes",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "GroupEventSlots",
                newName: "GroupEventSlots",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "GroupEvents",
                newName: "GroupEvents",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "GroupEventCantPlays",
                newName: "GroupEventCantPlays",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "DataProtectionKeys",
                newName: "DataProtectionKeys",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "AspNetUserTokens",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "AspNetUsers",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "AspNetUserRoles",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "AspNetUserLogins",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "AspNetUserClaims",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "AspNetRoles",
                newSchema: "private");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "AspNetRoleClaims",
                newSchema: "private");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UserPushSubscription",
                schema: "private",
                newName: "UserPushSubscription");

            migrationBuilder.RenameTable(
                name: "Memberships",
                schema: "private",
                newName: "Memberships");

            migrationBuilder.RenameTable(
                name: "Groups",
                schema: "private",
                newName: "Groups");

            migrationBuilder.RenameTable(
                name: "GroupInvites",
                schema: "private",
                newName: "GroupInvites");

            migrationBuilder.RenameTable(
                name: "GroupGames",
                schema: "private",
                newName: "GroupGames");

            migrationBuilder.RenameTable(
                name: "GroupEventSlotVotes",
                schema: "private",
                newName: "GroupEventSlotVotes");

            migrationBuilder.RenameTable(
                name: "GroupEventSlots",
                schema: "private",
                newName: "GroupEventSlots");

            migrationBuilder.RenameTable(
                name: "GroupEvents",
                schema: "private",
                newName: "GroupEvents");

            migrationBuilder.RenameTable(
                name: "GroupEventCantPlays",
                schema: "private",
                newName: "GroupEventCantPlays");

            migrationBuilder.RenameTable(
                name: "DataProtectionKeys",
                schema: "private",
                newName: "DataProtectionKeys");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                schema: "private",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                schema: "private",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                schema: "private",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                schema: "private",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                schema: "private",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                schema: "private",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                schema: "private",
                newName: "AspNetRoleClaims");
        }
    }
}
