using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLBVo.Migrations
{
    public partial class AddMemberCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Members",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Members",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Members",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // Migrate dữ liệu cũ — tự động phân loại
            migrationBuilder.Sql(@"
                UPDATE Members SET Category = 
                    CASE
                        WHEN Role LIKE N'%Huấn luyện%' 
                          OR Role LIKE N'%HLV%' 
                          OR Role LIKE N'%Coach%' 
                          OR Role LIKE N'%Trợ giảng%' 
                          OR Role LIKE N'%Giáo viên%'
                            THEN N'HLV'
                        WHEN Description LIKE N'%Kickboxing%' 
                          OR Role LIKE N'%Kickboxing%'
                            THEN N'Kickboxing'
                        WHEN Description LIKE N'%Boxing%' 
                          OR Role LIKE N'%Boxing%'
                            THEN N'Boxing'
                        ELSE N'Boxing'
                    END
                WHERE Category IS NULL
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Members");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Members",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Members",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);
        }
    }
}