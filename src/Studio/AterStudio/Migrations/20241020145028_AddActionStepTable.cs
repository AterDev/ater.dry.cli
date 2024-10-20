using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AterStudio.Migrations
{
    /// <inheritdoc />
    public partial class AddActionStepTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenActionGenStep");

            migrationBuilder.CreateTable(
                name: "GenActionGenSteps",
                columns: table => new
                {
                    GenActionsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GenStepsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenActionGenSteps", x => new { x.GenActionsId, x.GenStepsId });
                    table.ForeignKey(
                        name: "FK_GenActionGenSteps_GenActions_GenActionsId",
                        column: x => x.GenActionsId,
                        principalTable: "GenActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GenActionGenSteps_GenSteps_GenStepsId",
                        column: x => x.GenStepsId,
                        principalTable: "GenSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GenActionGenSteps_GenStepsId",
                table: "GenActionGenSteps",
                column: "GenStepsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenActionGenSteps");

            migrationBuilder.CreateTable(
                name: "GenActionGenStep",
                columns: table => new
                {
                    GenActionsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GenStepsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenActionGenStep", x => new { x.GenActionsId, x.GenStepsId });
                    table.ForeignKey(
                        name: "FK_GenActionGenStep_GenActions_GenActionsId",
                        column: x => x.GenActionsId,
                        principalTable: "GenActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GenActionGenStep_GenSteps_GenStepsId",
                        column: x => x.GenStepsId,
                        principalTable: "GenSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GenActionGenStep_GenStepsId",
                table: "GenActionGenStep",
                column: "GenStepsId");
        }
    }
}
