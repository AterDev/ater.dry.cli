using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastore.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertySummarycomment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Comments",
                table: "PropertyInfos",
                newName: "CommentXml");

            migrationBuilder.AddColumn<string>(
                name: "CommentSummary",
                table: "PropertyInfos",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentSummary",
                table: "PropertyInfos");

            migrationBuilder.RenameColumn(
                name: "CommentXml",
                table: "PropertyInfos",
                newName: "Comments");
        }
    }
}
