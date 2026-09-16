using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CommunityBot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDurableActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activity_capture_gates",
                columns: table => new
                {
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_capture_gates", x => x.event_type);
                });

            migrationBuilder.CreateTable(
                name: "activity_events",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    member_id = table.Column<long>(type: "bigint", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    occurrence_count = table.Column<int>(type: "integer", nullable: false),
                    captured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    contract_version = table.Column<int>(type: "integer", nullable: false),
                    source_reference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_events", x => x.id);
                    table.CheckConstraint("CK_activity_events_contract_version_positive", "contract_version > 0");
                    table.CheckConstraint("CK_activity_events_event_id_not_empty", "event_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                    table.CheckConstraint("CK_activity_events_occurrence_count_positive", "occurrence_count > 0");
                    table.CheckConstraint("CK_activity_events_source_reference_not_blank", "source_reference IS NULL OR length(btrim(source_reference)) > 0");
                    table.ForeignKey(
                        name: "fk_activity_events_activity_capture_gates_event_type",
                        column: x => x.event_type,
                        principalTable: "activity_capture_gates",
                        principalColumn: "event_type",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_activity_events_members_member_id",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "activity_subscriptions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    consumer_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    context_reference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    capture_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    capture_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_subscriptions", x => x.id);
                    table.CheckConstraint("CK_activity_subscriptions_context_reference_not_blank", "length(btrim(context_reference)) > 0");
                    table.CheckConstraint("CK_activity_subscriptions_valid_window", "capture_until > capture_from");
                    table.ForeignKey(
                        name: "fk_activity_subscriptions_activity_capture_gates_event_type",
                        column: x => x.event_type,
                        principalTable: "activity_capture_gates",
                        principalColumn: "event_type",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "activity_consumptions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    activity_event_id = table.Column<long>(type: "bigint", nullable: false),
                    subscription_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_attempt_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    next_attempt_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_consumptions", x => x.id);
                    table.CheckConstraint("CK_activity_consumptions_attempt_count_non_negative", "attempt_count >= 0");
                    table.ForeignKey(
                        name: "fk_activity_consumptions_activity_events_activity_event_id",
                        column: x => x.activity_event_id,
                        principalTable: "activity_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_activity_consumptions_activity_subscriptions_subscription_id",
                        column: x => x.subscription_id,
                        principalTable: "activity_subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "activity_capture_gates",
                column: "event_type",
                value: "ShopPurchaseCompleted");

            migrationBuilder.CreateIndex(
                name: "ix_activity_consumptions_pending_queue",
                table: "activity_consumptions",
                columns: new[] { "created_at", "id" },
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "ix_activity_consumptions_subscription_id",
                table: "activity_consumptions",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "ux_activity_consumptions_event_subscription",
                table: "activity_consumptions",
                columns: new[] { "activity_event_id", "subscription_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_activity_events_event_type",
                table: "activity_events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "ix_activity_events_member_id",
                table: "activity_events",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "ux_activity_events_event_id",
                table: "activity_events",
                column: "event_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_activity_subscriptions_event_type",
                table: "activity_subscriptions",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "ux_activity_subscriptions_consumer_context_event_type",
                table: "activity_subscriptions",
                columns: new[] { "consumer_type", "context_reference", "event_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_consumptions");

            migrationBuilder.DropTable(
                name: "activity_events");

            migrationBuilder.DropTable(
                name: "activity_subscriptions");

            migrationBuilder.DropTable(
                name: "activity_capture_gates");
        }
    }
}
