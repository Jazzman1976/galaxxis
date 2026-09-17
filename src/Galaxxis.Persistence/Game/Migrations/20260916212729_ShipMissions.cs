using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Galaxxis.Persistence.Game.Migrations
{
    /// <inheritdoc />
    public partial class ShipMissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MissionEventId",
                table: "Ships",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MissionEventId",
                table: "Ships");
        }
    }
}
