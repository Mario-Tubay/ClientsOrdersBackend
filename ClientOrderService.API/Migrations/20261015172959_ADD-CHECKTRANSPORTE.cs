using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientOrderService.API.Migrations
{
    /// <inheritdoc />
    public partial class ADDCHECKTRANSPORTE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CheckTransporte",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckTransporte",
                table: "Orders");
        }
    }
}
