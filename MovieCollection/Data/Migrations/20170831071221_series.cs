using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MovieCollection.Data.Migrations
{
    public partial class series : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdSerie",
                table: "Movies",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    IdSerie = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OriginalName = table.Column<string>(nullable: true),
                    SerieName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => x.IdSerie);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movies_IdSerie",
                table: "Movies",
                column: "IdSerie");

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_Series_IdSerie",
                table: "Movies",
                column: "IdSerie",
                principalTable: "Series",
                principalColumn: "IdSerie",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movies_Series_IdSerie",
                table: "Movies");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropIndex(
                name: "IX_Movies_IdSerie",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "IdSerie",
                table: "Movies");
        }
    }
}
