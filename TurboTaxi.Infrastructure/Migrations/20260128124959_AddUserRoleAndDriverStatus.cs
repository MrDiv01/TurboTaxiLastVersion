using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurboTaxi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoleAndDriverStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "CurrentLatitude",
                table: "Drivers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrentLongitude",
                table: "Drivers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Drivers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Drivers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CurrentLatitude",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "CurrentLongitude",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Drivers");
        }
    }
}
