using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AterStudio.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGenStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "GenSteps",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "GenSteps");
        }
    }
}
