using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingManagementTool.Migrations
{
    /// <inheritdoc />
    public partial class AddedPropertyCategoryIdToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PropertyCategoryId",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PropertyCategoryId",
                table: "Documents",
                column: "PropertyCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_PropertyCategories_PropertyCategoryId",
                table: "Documents",
                column: "PropertyCategoryId",
                principalTable: "PropertyCategories",
                principalColumn: "PropertyCategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_PropertyCategories_PropertyCategoryId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_PropertyCategoryId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "PropertyCategoryId",
                table: "Documents");
        }
    }
}
