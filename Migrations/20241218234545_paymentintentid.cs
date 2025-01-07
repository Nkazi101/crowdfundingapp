using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crowdfunding.Migrations
{
    /// <inheritdoc />
    public partial class paymentintentid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentID",
                table: "Pledges",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentIntentID",
                table: "Pledges");
        }
    }
}
