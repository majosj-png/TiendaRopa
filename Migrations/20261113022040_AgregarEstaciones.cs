using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TiendaRopa.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstacionId",
                table: "Prendas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Estaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estaciones", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Estaciones",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Verano" },
                    { 2, "Primavera" },
                    { 3, "Otoño" },
                    { 4, "Invierno" }
                });

            migrationBuilder.UpdateData(
                table: "Prendas",
                keyColumn: "Id",
                keyValue: 1,
                column: "EstacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Prendas",
                keyColumn: "Id",
                keyValue: 2,
                column: "EstacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Prendas",
                keyColumn: "Id",
                keyValue: 3,
                column: "EstacionId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Prendas",
                keyColumn: "Id",
                keyValue: 4,
                column: "EstacionId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Prendas",
                keyColumn: "Id",
                keyValue: 5,
                column: "EstacionId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Prendas",
                keyColumn: "Id",
                keyValue: 6,
                column: "EstacionId",
                value: 4);

            // Asigna una estación por defecto a cualquier prenda existente que no esté en el seed
            // (por ejemplo, creada por la API) para poder crear la FK sin errores.
            migrationBuilder.Sql("""
                UPDATE "Prendas" SET "EstacionId" = 1 WHERE "EstacionId" = 0;
                """);

            // Ajusta la secuencia de la identity tras insertar el seed con Ids explícitos.
            migrationBuilder.Sql("""
                SELECT setval(pg_get_serial_sequence('"Estaciones"', 'Id'), COALESCE(MAX("Id"), 1), MAX("Id") IS NOT NULL) FROM "Estaciones";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Prendas_EstacionId",
                table: "Prendas",
                column: "EstacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prendas_Estaciones_EstacionId",
                table: "Prendas",
                column: "EstacionId",
                principalTable: "Estaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prendas_Estaciones_EstacionId",
                table: "Prendas");

            migrationBuilder.DropTable(
                name: "Estaciones");

            migrationBuilder.DropIndex(
                name: "IX_Prendas_EstacionId",
                table: "Prendas");

            migrationBuilder.DropColumn(
                name: "EstacionId",
                table: "Prendas");
        }
    }
}
