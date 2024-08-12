using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingManagementTool.Migrations
{
    /// <inheritdoc />
    public partial class addedIconUrlToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "IconUrl",
                table: "Documents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "Documents");
        }
    }
}
