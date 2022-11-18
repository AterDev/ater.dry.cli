using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Studio.Migrations;

/// <inheritdoc />
public partial class Init : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.CreateTable(
            name: "Projects",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                Path = table.Column<string>(type: "TEXT", nullable: false),
                EntityPath = table.Column<string>(type: "TEXT", nullable: false),
                SharePath = table.Column<string>(type: "TEXT", nullable: false),
                ApplicationPath = table.Column<string>(type: "TEXT", nullable: false),
                HttpPath = table.Column<string>(type: "TEXT", nullable: false),
                EntityFrameworkPath = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Projects", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "Projects");
    }
}
