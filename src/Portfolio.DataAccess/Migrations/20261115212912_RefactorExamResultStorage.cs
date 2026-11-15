using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RefactorExamResultStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_Exams_ExamId",
                table: "ExamResults");

            migrationBuilder.DropTable(
                name: "ExamQuestionAlternatives");

            migrationBuilder.DropTable(
                name: "ExamQuestionLinks");

            migrationBuilder.DropTable(
                name: "ExamResultAnswers");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "ExamQuestions");

            migrationBuilder.DropIndex(
                name: "IX_ExamResults_ExamId",
                table: "ExamResults");

            migrationBuilder.AddColumn<string>(
                name: "Answers",
                table: "ExamResults",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.Sql(
                "ALTER TABLE \"ExamResults\" ALTER COLUMN \"Answers\" DROP DEFAULT;");

            migrationBuilder.AddColumn<bool>(
                name: "Humanities",
                table: "ExamResults",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "ExamResults",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Languages",
                table: "ExamResults",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Math",
                table: "ExamResults",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Nature",
                table: "ExamResults",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "QuestionCount",
                table: "ExamResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimeInMinutes",
                table: "ExamResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ExamResults",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "ExamResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answers",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "Humanities",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "Languages",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "Math",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "Nature",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "QuestionCount",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "TimeInMinutes",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "ExamResults");

            migrationBuilder.CreateTable(
                name: "ExamQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlternativesIntroduction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Context = table.Column<string>(type: "text", nullable: true),
                    CorrectAlternative = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Discipline = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Files = table.Column<string>(type: "jsonb", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Humanities = table.Column<bool>(type: "boolean", nullable: false),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Languages = table.Column<bool>(type: "boolean", nullable: false),
                    Math = table.Column<bool>(type: "boolean", nullable: false),
                    Nature = table.Column<bool>(type: "boolean", nullable: false),
                    QuestionCount = table.Column<int>(type: "integer", nullable: false),
                    TimeInMinutes = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamQuestionAlternatives",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Letter = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    File = table.Column<string>(type: "text", nullable: true),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamQuestionAlternatives", x => new { x.QuestionId, x.Letter });
                    table.ForeignKey(
                        name: "FK_ExamQuestionAlternatives_ExamQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "ExamQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamResultAnswers",
                columns: table => new
                {
                    ResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedAlternativeLetter = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResultAnswers", x => new { x.ResultId, x.QuestionId });
                    table.ForeignKey(
                        name: "FK_ExamResultAnswers_ExamQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "ExamQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamResultAnswers_ExamResults_ResultId",
                        column: x => x.ResultId,
                        principalTable: "ExamResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamQuestionLinks",
                columns: table => new
                {
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamQuestionLinks", x => new { x.ExamId, x.QuestionId });
                    table.ForeignKey(
                        name: "FK_ExamQuestionLinks_ExamQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "ExamQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamQuestionLinks_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_ExamId",
                table: "ExamResults",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestionLinks_ExamId_Position",
                table: "ExamQuestionLinks",
                columns: new[] { "ExamId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestionLinks_QuestionId",
                table: "ExamQuestionLinks",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResultAnswers_QuestionId",
                table: "ExamResultAnswers",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_Exams_ExamId",
                table: "ExamResults",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
