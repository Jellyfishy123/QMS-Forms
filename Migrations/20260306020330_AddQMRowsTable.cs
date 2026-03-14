using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QMSForms.Migrations
{
    /// <inheritdoc />
    public partial class AddQMRowsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QMRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Question = table.Column<string>(type: "text", nullable: false),
                    Ideal = table.Column<int>(type: "integer", nullable: false),
                    Actual = table.Column<int>(type: "integer", nullable: true),
                    ObservationRemarks = table.Column<string>(type: "text", nullable: false),
                    AssessmentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QMRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QMRows_IMSAssessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "IMSAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QMRows_AssessmentId",
                table: "QMRows",
                column: "AssessmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QMRows");
        }
    }
}
