using Microsoft.EntityFrameworkCore.Migrations;

namespace knowledge_world_dharma_backend.Migrations
{
    public partial class Add_Annoucements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Annoucement",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Admin = table.Column<int>(nullable: false),
                    Post = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Annoucement", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Annoucement");
        }
    }
}
