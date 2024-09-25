using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingManagementTool.Migrations
{
    /// <inheritdoc />
    public partial class addIsActiveNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActiveNote",
                table: "Documents",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActiveNote",
                table: "Documents");
        }
    }
}
