using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CommunityBot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityRecovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activity_capture_incidents",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    member_id = table.Column<long>(type: "bigint", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    occurrence_count = table.Column<int>(type: "integer", nullable: false),
                    source_reference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    detected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    nominal_error = table.Column<string>(type: "text", nullable: false),
                    conservative_error = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_capture_incidents", x => x.id);
                    table.CheckConstraint("CK_activity_capture_incidents_conservative_error_not_blank", "length(btrim(conservative_error)) > 0");
                    table.CheckConstraint("CK_activity_capture_incidents_nominal_error_not_blank", "length(btrim(nominal_error)) > 0");
                    table.CheckConstraint("CK_activity_capture_incidents_occurrence_count_positive", "occurrence_count > 0");
                    table.CheckConstraint("CK_activity_capture_incidents_source_reference_not_blank", "source_reference IS NULL OR length(btrim(source_reference)) > 0");
                });

            migrationBuilder.CreateTable(
                name: "activity_reconciliations",
                columns: table => new
                {
                    activity_event_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_reconciliations", x => x.activity_event_id);
                    table.ForeignKey(
                        name: "fk_activity_reconciliations_activity_events_activity_event_id",
                        column: x => x.activity_event_id,
                        principalTable: "activity_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_activity_capture_incidents_detected_at",
                table: "activity_capture_incidents",
                column: "detected_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_capture_incidents");

            migrationBuilder.DropTable(
                name: "activity_reconciliations");
        }
    }
}
