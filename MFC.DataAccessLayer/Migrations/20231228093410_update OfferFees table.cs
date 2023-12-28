using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MFC.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class updateOfferFeestable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OfferFeeId",
                table: "OfferFees",
                newName: "OfferId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OfferId",
                table: "OfferFees",
                newName: "OfferFeeId");
        }
    }
}
