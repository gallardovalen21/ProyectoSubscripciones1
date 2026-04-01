using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clasess.Migrations
{
    /// <inheritdoc />
    public partial class AddHouseAndSubscriptionHouseId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Periodo",
                table: "Pagos");

            migrationBuilder.AddColumn<int>(
                name: "HouseId",
                table: "Subscriptions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Houses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Houses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_HouseId",
                table: "Subscriptions",
                column: "HouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Houses_HouseId",
                table: "Subscriptions",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Houses_HouseId",
                table: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Houses");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_HouseId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "HouseId",
                table: "Subscriptions");

            migrationBuilder.AddColumn<string>(
                name: "Periodo",
                table: "Pagos",
                type: "TEXT",
                nullable: true);
        }
    }
}
