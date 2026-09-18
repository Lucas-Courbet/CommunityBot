using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunityBot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "community_goals",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    target_count = table.Column<int>(type: "integer", nullable: false),
                    current_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_community_goals", x => x.id);
                    table.CheckConstraint("CK_community_goals_current_count_valid", "current_count >= 0 AND current_count <= target_count");
                    table.CheckConstraint("CK_community_goals_id_not_blank", "length(btrim(id)) > 0");
                    table.CheckConstraint("CK_community_goals_target_count_positive", "target_count > 0");
                    table.CheckConstraint("CK_community_goals_title_not_blank", "length(btrim(title)) > 0");
                    table.CheckConstraint("CK_community_goals_valid_window", "ends_at > starts_at");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "community_goals");
        }
    }
}
