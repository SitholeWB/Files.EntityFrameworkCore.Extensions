using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SQLite.WebApi.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoodImage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    NextId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ChunkBytesLength = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalBytesLength = table.Column<long>(type: "INTEGER", nullable: false),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodImage", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodImage");
        }
    }
}
