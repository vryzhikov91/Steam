using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamGameParse.Data.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    Developer = table.Column<string>(nullable: false),
                    Published = table.Column<string>(nullable: false),
                    ReleaseDate = table.Column<string>(nullable: false),
                    ImagePath = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchApps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Appid = table.Column<string>(nullable: true),
                    ExecutionResult = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchApps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    AppDataId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Genres_AppData_AppDataId",
                        column: x => x.AppDataId,
                        principalTable: "AppData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Requirements",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemType = table.Column<int>(nullable: false),
                    RequirementsType = table.Column<int>(nullable: false),
                    Processor = table.Column<string>(nullable: true),
                    Memory = table.Column<string>(nullable: true),
                    Graphics = table.Column<string>(nullable: true),
                    Os = table.Column<string>(nullable: true),
                    Storage = table.Column<string>(nullable: true),
                    DirectX = table.Column<string>(nullable: true),
                    AppDataId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requirements_AppData_AppDataId",
                        column: x => x.AppDataId,
                        principalTable: "AppData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Genres_AppDataId",
                table: "Genres",
                column: "AppDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Requirements_AppDataId",
                table: "Requirements",
                column: "AppDataId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Requirements");

            migrationBuilder.DropTable(
                name: "SearchApps");

            migrationBuilder.DropTable(
                name: "AppData");
        }
    }
}
