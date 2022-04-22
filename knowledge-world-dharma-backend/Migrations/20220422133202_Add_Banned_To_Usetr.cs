using Microsoft.EntityFrameworkCore.Migrations;

namespace knowledge_world_dharma_backend.Migrations
{
    public partial class Add_Banned_To_Usetr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Banned",
                table: "UserModel",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Banned",
                table: "UserModel");
        }
    }
}
