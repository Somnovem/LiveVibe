using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveVibe.Server.Migrations
{
    /// <inheritdoc />
    public partial class OrderUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ticket_Purchases");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Tickets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "WasRefunded",
                table: "Tickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "WasRefunded",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "WasRefunded",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WasRefunded",
                table: "Orders");

            migrationBuilder.CreateTable(
                name: "Ticket_Purchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WasRefunded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket_Purchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ticket_Purchases_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ticket_Purchases_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_Purchases_TicketId",
                table: "Ticket_Purchases",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_Purchases_UserId",
                table: "Ticket_Purchases",
                column: "UserId");
        }
    }
}
