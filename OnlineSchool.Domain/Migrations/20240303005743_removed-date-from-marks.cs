using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSchool.Domain.Migrations
{
    /// <inheritdoc />
    public partial class removeddatefrommarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSubjectDateMark",
                table: "UserSubjectDateMark");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserSubjectDateMark");

            migrationBuilder.RenameTable(
                name: "UserSubjectDateMark",
                newName: "UserSubjectDateMarks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSubjectDateMarks",
                table: "UserSubjectDateMarks",
                columns: new[] { "UserId", "SubjectDateId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSubjectDateMarks",
                table: "UserSubjectDateMarks");

            migrationBuilder.RenameTable(
                name: "UserSubjectDateMarks",
                newName: "UserSubjectDateMark");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserSubjectDateMark",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSubjectDateMark",
                table: "UserSubjectDateMark",
                columns: new[] { "UserId", "SubjectDateId" });
        }
    }
}
