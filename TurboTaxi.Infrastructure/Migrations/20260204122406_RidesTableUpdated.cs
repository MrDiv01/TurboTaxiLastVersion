using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurboTaxi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RidesTableUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxDiscountAmount",
                table: "PromoCodes");

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "Rides",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "Rides");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxDiscountAmount",
                table: "PromoCodes",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
