using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Galaxxis.Persistence.Game.Migrations
{
    /// <inheritdoc />
    public partial class InitialGameSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Universes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GenerationSeed = table.Column<int>(type: "int", nullable: false),
                    Epoch = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TickLengthMilliseconds = table.Column<long>(type: "bigint", nullable: false),
                    PausedSince = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TotalPausedMilliseconds = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Galaxies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniverseId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false),
                    Z = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Galaxies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Galaxies_Universes_UniverseId",
                        column: x => x.UniverseId,
                        principalTable: "Universes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniverseId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Race = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Universes_UniverseId",
                        column: x => x.UniverseId,
                        principalTable: "Universes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StarSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GalaxyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false),
                    Z = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarSystems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StarSystems_Galaxies_GalaxyId",
                        column: x => x.GalaxyId,
                        principalTable: "Galaxies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Planets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniverseId = table.Column<int>(type: "int", nullable: false),
                    StarSystemId = table.Column<int>(type: "int", nullable: false),
                    OwnerPlayerId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Fields = table.Column<int>(type: "int", nullable: false),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false),
                    Z = table.Column<int>(type: "int", nullable: false),
                    ResourcesSettledTick = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Ore = table.Column<double>(type: "float", nullable: false),
                    Energy = table.Column<double>(type: "float", nullable: false),
                    Tools = table.Column<double>(type: "float", nullable: false),
                    Mana = table.Column<double>(type: "float", nullable: false),
                    Crystals = table.Column<double>(type: "float", nullable: false),
                    Antimatter = table.Column<double>(type: "float", nullable: false),
                    Elementium = table.Column<double>(type: "float", nullable: false),
                    OreMineLevel = table.Column<int>(type: "int", nullable: false),
                    OreMineSpeedPercent = table.Column<int>(type: "int", nullable: false),
                    PowerSupplyLevel = table.Column<int>(type: "int", nullable: false),
                    PowerSupplySpeedPercent = table.Column<int>(type: "int", nullable: false),
                    FactoryLevel = table.Column<int>(type: "int", nullable: false),
                    FactorySpeedPercent = table.Column<int>(type: "int", nullable: false),
                    MageAcademyLevel = table.Column<int>(type: "int", nullable: false),
                    MageAcademySpeedPercent = table.Column<int>(type: "int", nullable: false),
                    CrystalMineLevel = table.Column<int>(type: "int", nullable: false),
                    CrystalMineSpeedPercent = table.Column<int>(type: "int", nullable: false),
                    NeutronAcceleratorLevel = table.Column<int>(type: "int", nullable: false),
                    NeutronAcceleratorSpeedPercent = table.Column<int>(type: "int", nullable: false),
                    ElementiumSynthesizerLevel = table.Column<int>(type: "int", nullable: false),
                    ElementiumSynthesizerSpeedPercent = table.Column<int>(type: "int", nullable: false),
                    ShipyardLevel = table.Column<int>(type: "int", nullable: false),
                    ShipyardIsWorking = table.Column<bool>(type: "bit", nullable: false),
                    DefenseSystemLevel = table.Column<int>(type: "int", nullable: false),
                    DefenseSystemIsWorking = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Planets_Players_OwnerPlayerId",
                        column: x => x.OwnerPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Planets_StarSystems_StarSystemId",
                        column: x => x.StarSystemId,
                        principalTable: "StarSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Planets_Universes_UniverseId",
                        column: x => x.UniverseId,
                        principalTable: "Universes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Defenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    PlanetId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Strategy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WeaponSlots = table.Column<int>(type: "int", nullable: false),
                    ArmorSlots = table.Column<int>(type: "int", nullable: false),
                    PowerUpSlots = table.Column<int>(type: "int", nullable: false),
                    StructurePoints = table.Column<int>(type: "int", nullable: false),
                    CpuPower = table.Column<int>(type: "int", nullable: false),
                    AccuracyPercent = table.Column<int>(type: "int", nullable: false),
                    ShieldPercent = table.Column<int>(type: "int", nullable: false),
                    AttackPower = table.Column<int>(type: "int", nullable: false),
                    Armor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Defenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Defenses_Planets_PlanetId",
                        column: x => x.PlanetId,
                        principalTable: "Planets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Defenses_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GameEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniverseId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    PlanetId = table.Column<int>(type: "int", nullable: true),
                    Queue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequestedTick = table.Column<long>(type: "bigint", nullable: false),
                    ProcessAtTick = table.Column<long>(type: "bigint", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameEvents_Planets_PlanetId",
                        column: x => x.PlanetId,
                        principalTable: "Planets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GameEvents_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GameEvents_Universes_UniverseId",
                        column: x => x.UniverseId,
                        principalTable: "Universes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Moons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanetId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Fields = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Moons_Planets_PlanetId",
                        column: x => x.PlanetId,
                        principalTable: "Planets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    PlanetId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Strategy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WeaponSlots = table.Column<int>(type: "int", nullable: false),
                    ArmorSlots = table.Column<int>(type: "int", nullable: false),
                    PowerUpSlots = table.Column<int>(type: "int", nullable: false),
                    StructurePoints = table.Column<int>(type: "int", nullable: false),
                    CpuPower = table.Column<int>(type: "int", nullable: false),
                    AccuracyPercent = table.Column<int>(type: "int", nullable: false),
                    ShieldPercent = table.Column<int>(type: "int", nullable: false),
                    AttackPower = table.Column<int>(type: "int", nullable: false),
                    Armor = table.Column<int>(type: "int", nullable: false),
                    DriveStrength = table.Column<int>(type: "int", nullable: false),
                    ElementiumPerDistanceUnit = table.Column<int>(type: "int", nullable: false),
                    StorageCapacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ships_Planets_PlanetId",
                        column: x => x.PlanetId,
                        principalTable: "Planets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Ships_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Defenses_PlanetId",
                table: "Defenses",
                column: "PlanetId");

            migrationBuilder.CreateIndex(
                name: "IX_Defenses_PlayerId",
                table: "Defenses",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Galaxies_UniverseId",
                table: "Galaxies",
                column: "UniverseId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEvents_PlayerId_CreatedAt",
                table: "GameEvents",
                columns: new[] { "PlayerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GameEvents_UniverseId_State_ProcessAtTick_Id",
                table: "GameEvents",
                columns: new[] { "UniverseId", "State", "ProcessAtTick", "Id" });

            migrationBuilder.CreateIndex(
                name: "UX_GameEvents_OneScheduledPerPlanetQueue",
                table: "GameEvents",
                columns: new[] { "PlanetId", "Queue" },
                unique: true,
                filter: "[State] = N'Scheduled' AND [PlanetId] IS NOT NULL AND [Queue] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Moons_PlanetId",
                table: "Moons",
                column: "PlanetId");

            migrationBuilder.CreateIndex(
                name: "IX_Planets_OwnerPlayerId",
                table: "Planets",
                column: "OwnerPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Planets_StarSystemId",
                table: "Planets",
                column: "StarSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_Planets_UniverseId_OwnerPlayerId",
                table: "Planets",
                columns: new[] { "UniverseId", "OwnerPlayerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Players_UniverseId_AccountId",
                table: "Players",
                columns: new[] { "UniverseId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ships_PlanetId",
                table: "Ships",
                column: "PlanetId");

            migrationBuilder.CreateIndex(
                name: "IX_Ships_PlayerId",
                table: "Ships",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_StarSystems_GalaxyId",
                table: "StarSystems",
                column: "GalaxyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Defenses");

            migrationBuilder.DropTable(
                name: "GameEvents");

            migrationBuilder.DropTable(
                name: "Moons");

            migrationBuilder.DropTable(
                name: "Ships");

            migrationBuilder.DropTable(
                name: "Planets");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "StarSystems");

            migrationBuilder.DropTable(
                name: "Galaxies");

            migrationBuilder.DropTable(
                name: "Universes");
        }
    }
}
