using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLBVo.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegistrationCode",
                table: "Registrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationCode",
                table: "Registrations");
        }
    }
}
