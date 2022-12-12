using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastore.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntityInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NamespaceName = table.Column<string>(type: "TEXT", nullable: true),
                    AssemblyName = table.Column<string>(type: "TEXT", nullable: true),
                    Comment = table.Column<string>(type: "TEXT", nullable: true),
                    KeyType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
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
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    IsList = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsNavigation = table.Column<bool>(type: "INTEGER", nullable: false),
                    NavigationName = table.Column<string>(type: "TEXT", nullable: true),
                    HasMany = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsEnum = table.Column<bool>(type: "INTEGER", nullable: false),
                    AttributeText = table.Column<string>(type: "TEXT", nullable: true),
                    Comments = table.Column<string>(type: "TEXT", nullable: true),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsNullable = table.Column<bool>(type: "INTEGER", nullable: false),
                    MinLength = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxLength = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDecimal = table.Column<bool>(type: "INTEGER", nullable: false),
                    SuffixContent = table.Column<string>(type: "TEXT", nullable: true),
                    EntityInfoId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyInfos_EntityInfos_EntityInfoId",
                        column: x => x.EntityInfoId,
                        principalTable: "EntityInfos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntityInfos_Name",
                table: "EntityInfos",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_EntityInfos_NamespaceName",
                table: "EntityInfos",
                column: "NamespaceName");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyInfos_EntityInfoId",
                table: "PropertyInfos",
                column: "EntityInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyInfos_Name",
                table: "PropertyInfos",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyInfos_Type",
                table: "PropertyInfos",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "PropertyInfos");

            migrationBuilder.DropTable(
                name: "EntityInfos");
        }
    }
}
