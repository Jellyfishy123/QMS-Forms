using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QMSForms.Migrations
{
    /// <inheritdoc />
    public partial class RequestType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestType",
                table: "Requests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestType",
                table: "Requests");
        }
    }
}
