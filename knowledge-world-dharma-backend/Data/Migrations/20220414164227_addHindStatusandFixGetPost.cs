using Microsoft.EntityFrameworkCore.Migrations;

namespace knowledge_world_dharma_backend.Data.Migrations
{
    public partial class addHindStatusandFixGetPost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HideStatus",
                table: "Post",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HideStatus",
                table: "Post");
        }
    }
}
