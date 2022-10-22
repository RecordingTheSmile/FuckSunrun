using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FuckSunrun.Migrations
{
    public partial class dev202210121 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sunrun_user",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    imei_code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    school_name = table.Column<string>(type: "text", nullable: false),
                    min_speed = table.Column<double>(type: "double precision", nullable: false),
                    max_speed = table.Column<double>(type: "double precision", nullable: false),
                    length = table.Column<int>(type: "integer", nullable: false),
                    latitude = table.Column<string>(type: "text", nullable: false),
                    longitude = table.Column<string>(type: "text", nullable: false),
                    step = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    is_enable = table.Column<bool>(type: "boolean", nullable: false),
                    fail_reason = table.Column<string>(type: "text", nullable: true),
                    belong_to = table.Column<long>(type: "bigint", nullable: false),
                    hour = table.Column<int>(type: "integer", nullable: false),
                    minute = table.Column<int>(type: "integer", nullable: false),
                    create_at = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sunrun_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "SunrunLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SunrunTaskId = table.Column<long>(type: "bigint", nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    FailReason = table.Column<string>(type: "text", nullable: true),
                    CreateAt = table.Column<long>(type: "bigint", nullable: false),
                    BelongTo = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SunrunLogs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sunrun_user");

            migrationBuilder.DropTable(
                name: "SunrunLogs");
        }
    }
}
