using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MFC.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class CreateOfferFeesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OfferFees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferFeeId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FeePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferFees", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OfferFees");
        }
    }
}
