using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CommunityBot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardEntitlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reward_entitlements",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    member_id = table.Column<long>(type: "bigint", nullable: false),
                    source_reference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    reward_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reward_reference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    last_attempt_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reward_entitlements", x => x.id);
                    table.CheckConstraint("CK_reward_entitlements_attempt_count_non_negative", "attempt_count >= 0");
                    table.CheckConstraint("CK_reward_entitlements_quantity_positive", "quantity > 0");
                    table.ForeignKey(
                        name: "fk_reward_entitlements_members_member_id",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reward_entitlements_member_id",
                table: "reward_entitlements",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "ix_reward_entitlements_source_reference",
                table: "reward_entitlements",
                column: "source_reference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reward_entitlements");
        }
    }
}
