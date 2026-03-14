using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QMSForms.Migrations
{
    /// <inheritdoc />
    public partial class AddClosureFieldsToRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ApprovalDecision",
                table: "Requests",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ClosureBy",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosureComments",
                table: "Requests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosureBy",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ClosureComments",
                table: "Requests");

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalDecision",
                table: "Requests",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
