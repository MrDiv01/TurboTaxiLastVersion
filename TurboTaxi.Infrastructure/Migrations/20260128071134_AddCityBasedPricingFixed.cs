using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TurboTaxi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCityBasedPricingFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Tariffs");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "Tariffs",
                newName: "DisplayCityName");

            migrationBuilder.AddColumn<string>(
                name: "CityKey",
                table: "Tariffs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Tariffs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Tariffs",
                columns: new[] { "Id", "BaseFare", "CancellationFee", "CityKey", "CountryCode", "CreatedTime", "DisplayCityName", "FreeKm", "FreeMinutes", "IsActive", "MinimumFare", "NightEnd", "NightMultiplier", "NightStart", "PricePerKm", "PricePerMinute", "UpdatedTime", "ValidFrom", "ValidTo", "VehicleType", "WaitingPricePerMinute" },
                values: new object[,]
                {
                    { 1, 180m, null, "moscow", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Москва", null, null, true, 350m, null, null, null, 28m, 9m, null, null, null, 0, null },
                    { 2, 160m, null, "saint_petersburg", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Санкт-Петербург", null, null, true, 320m, null, null, null, 25m, 8m, null, null, null, 0, null },
                    { 3, 130m, null, "yekaterinburg", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Екатеринбург", null, null, true, 280m, null, null, null, 22m, 7m, null, null, null, 0, null },
                    { 4, 130m, null, "novosibirsk", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Новосибирск", null, null, true, 280m, null, null, null, 22m, 7m, null, null, null, 0, null },
                    { 5, 120m, null, "kazan", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Казань", null, null, true, 260m, null, null, null, 20m, 6m, null, null, null, 0, null },
                    { 6, 120m, null, "krasnoyarsk", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Красноярск", null, null, true, 260m, null, null, null, 20m, 6m, null, null, null, 0, null },
                    { 7, 125m, null, "nizhny_novgorod", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Нижний Новгород", null, null, true, 270m, null, null, null, 21m, 6m, null, null, null, 0, null },
                    { 8, 120m, null, "chelyabinsk", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Челябинск", null, null, true, 260m, null, null, null, 20m, 6m, null, null, null, 0, null },
                    { 9, 115m, null, "ufa", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Уфа", null, null, true, 250m, null, null, null, 19m, 6m, null, null, null, 0, null },
                    { 10, 120m, null, "samara", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Самара", null, null, true, 260m, null, null, null, 20m, 6m, null, null, null, 0, null },
                    { 11, 125m, null, "rostov_on_don", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ростов-на-Дону", null, null, true, 270m, null, null, null, 21m, 6m, null, null, null, 0, null },
                    { 12, 130m, null, "krasnodar", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Краснодар", null, null, true, 280m, null, null, null, 22m, 7m, null, null, null, 0, null },
                    { 13, 115m, null, "omsk", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Омск", null, null, true, 250m, null, null, null, 19m, 6m, null, null, null, 0, null },
                    { 14, 120m, null, "voronezh", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Воронеж", null, null, true, 260m, null, null, null, 20m, 6m, null, null, null, 0, null },
                    { 15, 125m, null, "perm", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Пермь", null, null, true, 270m, null, null, null, 21m, 6m, null, null, null, 0, null },
                    { 16, 120m, null, "volgograd", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Волгоград", null, null, true, 260m, null, null, null, 20m, 6m, null, null, null, 0, null },
                    { 17, 130m, null, "tyumen", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Тюмень", null, null, true, 280m, null, null, null, 22m, 7m, null, null, null, 0, null },
                    { 18, 115m, null, "saratov", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Саратов", null, null, true, 250m, null, null, null, 19m, 6m, null, null, null, 0, null },
                    { 19, 110m, null, "dagestan_avg", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Дагестан (orta)", null, null, true, 230m, null, null, null, 18m, 5m, null, null, null, 0, null },
                    { 20, 200m, null, "norilsk", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Норильск", null, null, true, 400m, null, null, null, 35m, 12m, null, null, null, 0, null },
                    { 21, 120m, null, "default_ru", "RU", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Russia Default", null, null, true, 260m, null, null, null, 20m, 6m, null, null, null, 0, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_CityKey_CountryCode",
                table: "Tariffs",
                columns: new[] { "CityKey", "CountryCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tariffs_CityKey_CountryCode",
                table: "Tariffs");

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Tariffs",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DropColumn(
                name: "CityKey",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Tariffs");

            migrationBuilder.RenameColumn(
                name: "DisplayCityName",
                table: "Tariffs",
                newName: "Country");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Tariffs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
