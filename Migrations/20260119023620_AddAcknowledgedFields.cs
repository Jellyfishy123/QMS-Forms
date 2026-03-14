using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QMSForms.Migrations
{
    /// <inheritdoc />
    public partial class AddAcknowledgedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrectivePreventiveAction",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionOfRootCause",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PMOrCM",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeOfRootCause",
                table: "Requests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectivePreventiveAction",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DescriptionOfRootCause",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "PMOrCM",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "TypeOfRootCause",
                table: "Requests");
        }
    }
}
