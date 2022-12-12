using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastore.Migrations
{
    /// <inheritdoc />
    public partial class ProjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectId",
                table: "PropertyInfos",
                type: "TEXT",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectId",
                table: "Projects",
                type: "TEXT",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectId",
                table: "EntityInfos",
                type: "TEXT",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyInfos_ProjectId",
                table: "PropertyInfos",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityInfos_ProjectId",
                table: "EntityInfos",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PropertyInfos_ProjectId",
                table: "PropertyInfos");

            migrationBuilder.DropIndex(
                name: "IX_EntityInfos_ProjectId",
                table: "EntityInfos");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "PropertyInfos");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "EntityInfos");
        }
    }
}
