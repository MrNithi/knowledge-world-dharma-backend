using Microsoft.EntityFrameworkCore.Migrations;

namespace knowledge_world_dharma_backend.Data.Migrations
{
    public partial class addHashtagInModelPost2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HashTag",
                table: "Post",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashTag",
                table: "Post");
        }
    }
}
