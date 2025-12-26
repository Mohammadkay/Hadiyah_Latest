using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HadiyahMigrations.Migrations
{
    /// <inheritdoc />
    public partial class AddRecivedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GiftArrivalDate",
                table: "OrderRecipients",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiftArrivalDate",
                table: "OrderRecipients");
        }
    }
}
