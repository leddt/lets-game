using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace LetsGame.Web.Data.Migrations
{
    public partial class SwitchToNodaTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "AvailabilityNotificationSentAt",
                table: "Memberships",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "AvailableUntil",
                table: "Memberships",
                type: "timestamp",
                nullable: true);

            migrationBuilder.Sql(@"UPDATE ""Memberships"" SET " +
                                 @"""AvailabilityNotificationSentAt"" = ""AvailabilityNotificationSentAtUtc"", " +
                                 @"""AvailableUntil"" = ""AvailableUntilUtc""");
            
            migrationBuilder.DropColumn(
                name: "AvailabilityNotificationSentAtUtc",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "AvailableUntilUtc",
                table: "Memberships");
            
            
            migrationBuilder.AddColumn<Instant>(
                name: "CreatedAt",
                table: "GroupInvites",
                type: "timestamp",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.Sql(@"UPDATE ""GroupInvites"" SET ""CreatedAt"" = ""CreatedAtUtc""");
            
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "GroupInvites");
            

            migrationBuilder.AddColumn<Instant>(
                name: "VotedAt",
                table: "GroupEventSlotVotes",
                type: "timestamp",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.Sql(@"UPDATE ""GroupEventSlotVotes"" SET ""VotedAt"" = ""VotedAtUtc""");

            migrationBuilder.DropColumn(
                name: "VotedAtUtc",
                table: "GroupEventSlotVotes");
            

            migrationBuilder.AddColumn<Instant>(
                name: "ProposedTime",
                table: "GroupEventSlots",
                type: "timestamp",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.Sql(@"UPDATE ""GroupEventSlots"" SET ""ProposedTime"" = ""ProposedDateAndTimeUtc""");

            migrationBuilder.DropColumn(
                name: "ProposedDateAndTimeUtc",
                table: "GroupEventSlots");
            

            migrationBuilder.AddColumn<Instant>(
                name: "AllVotesInNotificationSentAt",
                table: "GroupEvents",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "ChosenTime",
                table: "GroupEvents",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "ReminderSentAt",
                table: "GroupEvents",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "StartingSoonNotificationSentAt",
                table: "GroupEvents",
                type: "timestamp",
                nullable: true);

            migrationBuilder.Sql(@"UPDATE ""GroupEvents"" SET " +
                                 @"""AllVotesInNotificationSentAt"" = ""AllVotesInNotificationSentAtUtc"", " +
                                 @"""ChosenTime"" = ""ChosenDateAndTimeUtc"", " +
                                 @"""ReminderSentAt"" = ""ReminderSentAtUtc"", " +
                                 @"""StartingSoonNotificationSentAt"" = ""StartingSoonNotificationSentAtUtc""");

            migrationBuilder.DropColumn(
                name: "AllVotesInNotificationSentAtUtc",
                table: "GroupEvents");

            migrationBuilder.DropColumn(
                name: "ChosenDateAndTimeUtc",
                table: "GroupEvents");

            migrationBuilder.DropColumn(
                name: "ReminderSentAtUtc",
                table: "GroupEvents");

            migrationBuilder.DropColumn(
                name: "StartingSoonNotificationSentAtUtc",
                table: "GroupEvents");


            migrationBuilder.AddColumn<Instant>(
                name: "AddedAt",
                table: "GroupEventCantPlays",
                type: "timestamp",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.Sql(@"UPDATE ""GroupEventCantPlays"" SET ""AddedAt"" = ""AddedAtUtc""");
            
            migrationBuilder.DropColumn(
                name: "AddedAtUtc",
                table: "GroupEventCantPlays");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AvailabilityNotificationSentAtUtc",
                table: "Memberships",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableUntilUtc",
                table: "Memberships",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.Sql(@"UPDATE ""Memberships"" SET " +
                                 @"""AvailabilityNotificationSentAtUtc"" = ""AvailabilityNotificationSentAt"", " +
                                 @"""AvailableUntilUtc"" = ""AvailableUntil""");
            
            migrationBuilder.DropColumn(
                name: "AvailabilityNotificationSentAt",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "AvailableUntil",
                table: "Memberships");
            

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "GroupInvites",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql(@"UPDATE ""GroupInvites"" SET ""CreatedAtUtc"" = ""CreatedAt""");
            
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "GroupInvites");
            

            migrationBuilder.AddColumn<DateTime>(
                name: "VotedAtUtc",
                table: "GroupEventSlotVotes",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            
            migrationBuilder.Sql(@"UPDATE ""GroupEventSlotVotes"" SET ""VotedAtUtc"" = ""VotedAt""");

            migrationBuilder.DropColumn(
                name: "VotedAt",
                table: "GroupEventSlotVotes");
            

            migrationBuilder.AddColumn<DateTime>(
                name: "ProposedDateAndTimeUtc",
                table: "GroupEventSlots",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql(@"UPDATE ""GroupEventSlots"" SET ""ProposedDateAndTimeUtc"" = ""ProposedTime""");

            migrationBuilder.DropColumn(
                name: "ProposedTime",
                table: "GroupEventSlots");
            

            migrationBuilder.AddColumn<DateTime>(
                name: "AllVotesInNotificationSentAtUtc",
                table: "GroupEvents",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ChosenDateAndTimeUtc",
                table: "GroupEvents",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderSentAtUtc",
                table: "GroupEvents",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartingSoonNotificationSentAtUtc",
                table: "GroupEvents",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.Sql(@"UPDATE ""GroupEvents"" SET " +
                                 @"""AllVotesInNotificationSentAtUtc"" = ""AllVotesInNotificationSentAt"", " +
                                 @"""ChosenDateAndTimeUtc"" = ""ChosenTime"", " +
                                 @"""ReminderSentAtUtc"" = ""ReminderSentAt"", " +
                                 @"""StartingSoonNotificationSentAtUtc"" = ""StartingSoonNotificationSentAt""");

            migrationBuilder.DropColumn(
                name: "AllVotesInNotificationSentAt",
                table: "GroupEvents");

            migrationBuilder.DropColumn(
                name: "ChosenTime",
                table: "GroupEvents");

            migrationBuilder.DropColumn(
                name: "ReminderSentAt",
                table: "GroupEvents");

            migrationBuilder.DropColumn(
                name: "StartingSoonNotificationSentAt",
                table: "GroupEvents");
            

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedAtUtc",
                table: "GroupEventCantPlays",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql(@"UPDATE ""GroupEventCantPlays"" SET ""AddedAtUtc"" = ""AddedAt""");

            migrationBuilder.DropColumn(
                name: "AddedAt",
                table: "GroupEventCantPlays");
        }
    }
}
