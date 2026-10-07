using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLBVo.Migrations
{
    /// <inheritdoc />
    public partial class TournamentNameNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TournamentId",
                table: "TournamentResults",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "TournamentNameText",
                table: "TournamentResults",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TournamentNameText",
                table: "TournamentResults");

            migrationBuilder.AlterColumn<int>(
                name: "TournamentId",
                table: "TournamentResults",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
