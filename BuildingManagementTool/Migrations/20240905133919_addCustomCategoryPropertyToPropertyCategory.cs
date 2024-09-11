using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingManagementTool.Migrations
{
    /// <inheritdoc />
    public partial class addCustomCategoryPropertyToPropertyCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyCategories_Categories_CategoryId",
                table: "PropertyCategories");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "PropertyCategories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CustomCategory",
                table: "PropertyCategories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyCategories_Categories_CategoryId",
                table: "PropertyCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyCategories_Categories_CategoryId",
                table: "PropertyCategories");

            migrationBuilder.DropColumn(
                name: "CustomCategory",
                table: "PropertyCategories");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "PropertyCategories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyCategories_Categories_CategoryId",
                table: "PropertyCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
