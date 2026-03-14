using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QMSForms.Migrations
{
    /// <inheritdoc />
    public partial class AddIMSAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IMSAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Project = table.Column<string>(type: "text", nullable: false),
                    SiteRepresentative = table.Column<string>(type: "text", nullable: false),
                    DateOfAssessment = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssessmentNo = table.Column<string>(type: "text", nullable: false),
                    AssessorAuditor = table.Column<string>(type: "text", nullable: false),
                    OverallInspectionScore = table.Column<decimal>(type: "numeric", nullable: true),
                    SitePhotoPath = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IMSAssessments", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IMSAssessments");
        }
    }
}
