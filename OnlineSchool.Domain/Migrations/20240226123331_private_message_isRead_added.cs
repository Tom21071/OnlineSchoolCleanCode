using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSchool.Domain.Migrations
{
    /// <inheritdoc />
    public partial class private_message_isRead_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "PrivateMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "PrivateMessages");
        }
    }
}
