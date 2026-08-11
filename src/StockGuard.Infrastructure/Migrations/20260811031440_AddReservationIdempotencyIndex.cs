using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockGuard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationIdempotencyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IdempotencyKey",
                table: "StockReservations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_IdempotencyKey",
                table: "StockReservations",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockReservations_IdempotencyKey",
                table: "StockReservations");

            migrationBuilder.AlterColumn<string>(
                name: "IdempotencyKey",
                table: "StockReservations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
