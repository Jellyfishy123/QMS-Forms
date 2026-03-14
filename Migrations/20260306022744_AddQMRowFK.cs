using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QMSForms.Migrations
{
    /// <inheritdoc />
    public partial class AddQMRowFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QMRows_IMSAssessments_AssessmentId",
                table: "QMRows");

            migrationBuilder.RenameColumn(
                name: "AssessmentId",
                table: "QMRows",
                newName: "IMSAssessmentId");

            migrationBuilder.RenameIndex(
                name: "IX_QMRows_AssessmentId",
                table: "QMRows",
                newName: "IX_QMRows_IMSAssessmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_QMRows_IMSAssessments_IMSAssessmentId",
                table: "QMRows",
                column: "IMSAssessmentId",
                principalTable: "IMSAssessments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QMRows_IMSAssessments_IMSAssessmentId",
                table: "QMRows");

            migrationBuilder.RenameColumn(
                name: "IMSAssessmentId",
                table: "QMRows",
                newName: "AssessmentId");

            migrationBuilder.RenameIndex(
                name: "IX_QMRows_IMSAssessmentId",
                table: "QMRows",
                newName: "IX_QMRows_AssessmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_QMRows_IMSAssessments_AssessmentId",
                table: "QMRows",
                column: "AssessmentId",
                principalTable: "IMSAssessments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
