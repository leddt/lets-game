using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace LetsGame.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeDotnet7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Manual alter script to specify UTC TZ
            migrationBuilder.Sql(
            """
            ALTER TABLE "Memberships" ALTER COLUMN "AvailableUntil" TYPE timestamp with time zone USING "AvailableUntil" AT TIME ZONE 'UTC';
            ALTER TABLE "Memberships" ALTER COLUMN "AvailabilityNotificationSentAt" TYPE timestamp with time zone USING "AvailabilityNotificationSentAt" AT TIME ZONE 'UTC';
            ALTER TABLE "GroupInvites" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone USING "CreatedAt" AT TIME ZONE 'UTC';
            ALTER TABLE "GroupEventSlotVotes" ALTER COLUMN "VotedAt" TYPE timestamp with time zone USING "VotedAt" AT TIME ZONE 'UTC';
            ALTER TABLE "GroupEventSlots" ALTER COLUMN "ProposedTime" TYPE timestamp with time zone USING "ProposedTime" AT TIME ZONE 'UTC';
            ALTER TABLE "GroupEvents" ALTER COLUMN "StartingSoonNotificationSentAt" TYPE timestamp with time zone USING "StartingSoonNotificationSentAt" AT TIME ZONE 'UTC';
            ALTER TABLE "GroupEvents" ALTER COLUMN "ReminderSentAt" TYPE timestamp with time zone USING "ReminderSentAt" AT TIME ZONE 'UTC';
            ALTER TABLE "GroupEvents" ALTER COLUMN "ChosenTime" TYPE timestamp with time zone USING "ChosenTime" AT TIME ZONE 'UTC';
            ALTER TABLE "GroupEvents" ALTER COLUMN "AllVotesInNotificationSentAt" TYPE timestamp with time zone USING "AllVotesInNotificationSentAt" AT TIME ZONE 'UTC';
            ALTER TABLE "GroupEventCantPlays" ALTER COLUMN "AddedAt" TYPE timestamp with time zone USING "AddedAt" AT TIME ZONE 'UTC';
            """);
            
            // migrationBuilder.AlterColumn<Instant>(
            //     name: "AvailableUntil",
            //     table: "Memberships",
            //     type: "timestamp with time zone",
            //     nullable: true,
            //     oldClrType: typeof(Instant),
            //     oldType: "timestamp",
            //     oldNullable: true);
            //
            // migrationBuilder.AlterColumn<Instant>(
            //     name: "AvailabilityNotificationSentAt",
            //     table: "Memberships",
            //     type: "timestamp with time zone",
            //     nullable: true,
            //     oldClrType: typeof(Instant),
            //     oldType: "timestamp",
            //     oldNullable: true);
            //
            // migrationBuilder.AlterColumn<Instant>(
            //     name: "CreatedAt",
            //     table: "GroupInvites",
            //     type: "timestamp with time zone",
            //     nullable: false,
            //     oldClrType: typeof(Instant),
            //     oldType: "timestamp");
            //
            // migrationBuilder.AlterColumn<Instant>(
            //     name: "VotedAt",
            //     table: "GroupEventSlotVotes",
            //     type: "timestamp with time zone",
            //     nullable: false,
            //     oldClrType: typeof(Instant),
            //     oldType: "timestamp");
            //
            // migrationBuilder.AlterColumn<Instant>(
            //     name: "ProposedTime",
            //     table: "GroupEventSlots",
            //     type: "timestamp with time zone",
            //     nullable: false,
            //     oldClrType: typeof(Instant),
            //     oldType: "timestamp");
            //
            // migrationBuilder.AlterColumn<Instant>(
            //     name: "StartingSoonNotificationSentAt",
            //     table: "GroupEvents",
            //     type: "timestamp with time zone",
            //     nullable: true,
            //     oldClrType: typeof(Instant),
            //     oldType: "timestamp",
            //     oldNullable: true);
            //
            // migrationBuilder.AlterColumn<Instant>(
            //     name: "ReminderSentAt",
            //     table: "GroupEvents",
            //     type: "timestamp with time zone",
            //     nullable: true,
            //     oldClrType: typeof(Instant),
            //     oldType: "timestamp",
            //     oldNullable: true);
            //
            // migrationBuilder.AlterColumn<Instant>(
            //     name: "ChosenTime",
            //     table: "GroupEvents",
            //     type: "timestamp with time zone",
            //     nullable: true,
            //     oldClrType: typeof(Instant),
            //     oldType: "timestamp",
            //     oldNullable: true);
            //
            // migrationBuilder.AlterColumn<Instant>(
            //     name: "AllVotesInNotificationSentAt",
            //     table: "GroupEvents",
            //     type: "timestamp with time zone",
            //     nullable: true,
            //     oldClrType: typeof(Instant),
            //     oldType: "timestamp",
            //     oldNullable: true);
            //
            // migrationBuilder.AlterColumn<Instant>(
            //     name: "AddedAt",
            //     table: "GroupEventCantPlays",
            //     type: "timestamp with time zone",
            //     nullable: false,
            //     oldClrType: typeof(Instant),
            //     oldType: "timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Instant>(
                name: "AvailableUntil",
                table: "Memberships",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "AvailabilityNotificationSentAt",
                table: "Memberships",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "CreatedAt",
                table: "GroupInvites",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "VotedAt",
                table: "GroupEventSlotVotes",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "ProposedTime",
                table: "GroupEventSlots",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "StartingSoonNotificationSentAt",
                table: "GroupEvents",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "ReminderSentAt",
                table: "GroupEvents",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "ChosenTime",
                table: "GroupEvents",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "AllVotesInNotificationSentAt",
                table: "GroupEvents",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "AddedAt",
                table: "GroupEventCantPlays",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");
        }
    }
}
