using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManagementSystemApp.Data.Migrations
{
    public partial class AddRowVersionProperly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Vehicles
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Vehicles");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Vehicles",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            // Insurance
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Insurances");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Insurances",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            // Passport
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Passports");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Passports",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            // RegistrationCertificate
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RegistrationCertificates");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RegistrationCertificates",
                type: "rowversion",
                rowVersion: true,
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Vehicles
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Vehicles");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Vehicles",
                type: "varbinary(max)",
                nullable: false);

            // Insurance
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Insurances");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Insurances",
                type: "varbinary(max)",
                nullable: false);

            // Passport
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Passports");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Passports",
                type: "varbinary(max)",
                nullable: false);

            // RegistrationCertificate
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RegistrationCertificates");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RegistrationCertificates",
                type: "varbinary(max)",
                nullable: false);
        }
    }
}