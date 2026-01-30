using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserAuth.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedIpAddressInOtpVerifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "OtpVerifications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "OtpVerifications");
        }
    }
}
