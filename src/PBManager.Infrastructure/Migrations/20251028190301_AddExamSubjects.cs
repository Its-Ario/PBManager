using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExamSubjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Exams_ExamId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_ExamId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "Subjects");

            migrationBuilder.CreateTable(
                name: "ExamSubject",
                columns: table => new
                {
                    ExamsId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubjectsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSubject", x => new { x.ExamsId, x.SubjectsId });
                    table.ForeignKey(
                        name: "FK_ExamSubject_Exams_ExamsId",
                        column: x => x.ExamsId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamSubject_Subjects_SubjectsId",
                        column: x => x.SubjectsId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubject_SubjectsId",
                table: "ExamSubject",
                column: "SubjectsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamSubject");

            migrationBuilder.AddColumn<int>(
                name: "ExamId",
                table: "Subjects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_ExamId",
                table: "Subjects",
                column: "ExamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Exams_ExamId",
                table: "Subjects",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id");
        }
    }
}
