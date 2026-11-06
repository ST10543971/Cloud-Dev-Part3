using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenueBookingApp.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypesAndVenueAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.EventTypeId);
                });

            migrationBuilder.InsertData(
                table: "EventTypes",
                columns: new[] { "EventTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Conference" },
                    { 2, "Wedding" },
                    { 3, "Concert" },
                    { 4, "Exhibition" },
                    { 5, "Private Function" }
                });

            migrationBuilder.AddColumn<int>(
                name: "EventTypeId",
                table: "Venues",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Venues",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Venues_EventTypeId",
                table: "Venues",
                column: "EventTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_EventTypes_EventTypeId",
                table: "Venues",
                column: "EventTypeId",
                principalTable: "EventTypes",
                principalColumn: "EventTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Venues_EventTypes_EventTypeId",
                table: "Venues");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropIndex(
                name: "IX_Venues_EventTypeId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "EventTypeId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Venues");
        }
    }
}
