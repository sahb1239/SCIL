using Microsoft.EntityFrameworkCore.Migrations;

namespace SCILRunner.Migrations
{
    public partial class FlixProcessesDatapoints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FlixProcesses",
                table: "DataPoint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlixProcesses",
                table: "DataPoint");
        }
    }
}
