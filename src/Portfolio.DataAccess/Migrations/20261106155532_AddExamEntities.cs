using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Portfolio.DataAccess.Models;

#nullable disable

namespace Portfolio.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddExamEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamAttempts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExamId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false),
                    AnonymousToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedFinishAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeInMinutes = table.Column<int>(type: "integer", nullable: false),
                    QuestionCount = table.Column<int>(type: "integer", nullable: false),
                    SelectedLanguage = table.Column<string>(type: "text", nullable: true),
                    IncludeHumanities = table.Column<bool>(type: "boolean", nullable: false),
                    IncludeNature = table.Column<bool>(type: "boolean", nullable: false),
                    IncludeLanguages = table.Column<bool>(type: "boolean", nullable: false),
                    IncludeMath = table.Column<bool>(type: "boolean", nullable: false),
                    ServedQuestions = table.Column<List<AttemptQuestionDbModel>>(type: "jsonb", nullable: false),
                    Answers = table.Column<List<QuestionAnswerDbModel>>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    QuestionCountHumanities = table.Column<int>(type: "integer", nullable: false),
                    QuestionCountNature = table.Column<int>(type: "integer", nullable: false),
                    QuestionCountLanguages = table.Column<int>(type: "integer", nullable: false),
                    QuestionCountMath = table.Column<int>(type: "integer", nullable: false),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    StartCount = table.Column<int>(type: "integer", nullable: false),
                    FinishCount = table.Column<int>(type: "integer", nullable: false),
                    TotalQuestionsFinished = table.Column<int>(type: "integer", nullable: false),
                    LastStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastFinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Languages = table.Column<string>(type: "jsonb", nullable: false),
                    Disciplines = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamAttempts_ExamId",
                table: "ExamAttempts",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAttempts_IsAnonymous_ExpiresAt",
                table: "ExamAttempts",
                columns: new[] { "IsAnonymous", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Exams_IsSynced",
                table: "Exams",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_Year",
                table: "Exams",
                column: "Year",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamAttempts");

            migrationBuilder.DropTable(
                name: "Exams");
        }
    }
}
