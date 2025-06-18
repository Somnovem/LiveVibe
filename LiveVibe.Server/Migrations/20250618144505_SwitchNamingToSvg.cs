using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveVibe.Server.Migrations
{
    /// <inheritdoc />
    public partial class SwitchNamingToSvg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QRCodeUrl",
                table: "Tickets",
                newName: "QRCodeSvg");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QRCodeSvg",
                table: "Tickets",
                newName: "QRCodeUrl");
        }
    }
}
