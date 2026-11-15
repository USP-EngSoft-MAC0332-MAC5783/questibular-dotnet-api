using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MoveResultsToExamAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                WITH legacy_exams AS (
                    SELECT DISTINCT ON (result."Year")
                        replace(result."ExamId"::text, '-', '') AS "Id",
                        left(result."Title", 200) AS "Title",
                        result."Year"::text AS "Year",
                        result."Language",
                        result."StartedAt"
                    FROM "ExamResults" AS result
                    ORDER BY result."Year", result."StartedAt", result."Id"
                )
                INSERT INTO "Exams" (
                    "Id",
                    "Title",
                    "Year",
                    "QuestionCountHumanities",
                    "QuestionCountNature",
                    "QuestionCountLanguages",
                    "QuestionCountMath",
                    "IsSynced",
                    "StartCount",
                    "FinishCount",
                    "TotalQuestionsFinished",
                    "LastStartedAt",
                    "LastFinishedAt",
                    "Languages",
                    "Disciplines",
                    "CreatedAt"
                )
                SELECT
                    legacy."Id",
                    legacy."Title",
                    legacy."Year",
                    0,
                    0,
                    0,
                    0,
                    false,
                    0,
                    0,
                    0,
                    legacy."StartedAt",
                    NULL,
                    jsonb_build_array(legacy."Language"),
                    '[]'::jsonb,
                    legacy."StartedAt"
                FROM legacy_exams AS legacy
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "Exams" AS exam
                    WHERE exam."Year" = legacy."Year"
                )
                ON CONFLICT ("Year") DO NOTHING;

                INSERT INTO "ExamAttempts" (
                    "Id",
                    "ExamId",
                    "UserId",
                    "IsAnonymous",
                    "AnonymousToken",
                    "StartedAt",
                    "PlannedFinishAt",
                    "FinishedAt",
                    "ExpiresAt",
                    "TimeInMinutes",
                    "QuestionCount",
                    "SelectedLanguage",
                    "IncludeHumanities",
                    "IncludeNature",
                    "IncludeLanguages",
                    "IncludeMath",
                    "ServedQuestions",
                    "Answers",
                    "CreatedAt"
                )
                SELECT
                    replace(result."Id"::text, '-', ''),
                    exam."Id",
                    result."StudentId",
                    result."Anonymous",
                    result."AnonymousToken",
                    result."StartedAt",
                    result."StartedAt" + result."TimeInMinutes" * interval '1 minute',
                    result."FinishAt",
                    NULL,
                    result."TimeInMinutes",
                    result."QuestionCount",
                    result."Language",
                    result."Humanities",
                    result."Nature",
                    result."Languages",
                    result."Math",
                    COALESCE((
                        SELECT jsonb_agg(jsonb_build_object(
                            'Index', questions.question_index,
                            'Discipline', 'unknown',
                            'Language', NULL
                        ) ORDER BY questions.question_index)
                        FROM generate_series(1, result."QuestionCount") AS questions(question_index)
                    ), '[]'::jsonb),
                    COALESCE((
                        SELECT jsonb_agg(jsonb_build_object(
                            'Index', (answer.value->>'questionIndex')::integer,
                            'CorrectAlternative', '',
                            'SelectedAlternative', answer.value->>'selectedAlternativeLetter'
                        ) ORDER BY (answer.value->>'questionIndex')::integer)
                        FROM jsonb_array_elements(result."Answers") AS answer(value)
                    ), '[]'::jsonb),
                    result."StartedAt"
                FROM "ExamResults" AS result
                INNER JOIN "Exams" AS exam ON exam."Year" = result."Year"::text
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "ExamAttempts" AS attempt
                    WHERE attempt."Id" = replace(result."Id"::text, '-', '')
                );
                """);

            migrationBuilder.DropTable(
                name: "ExamResults");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Anonymous = table.Column<bool>(type: "boolean", nullable: false),
                    AnonymousToken = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Answers = table.Column<string>(type: "jsonb", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinishAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Humanities = table.Column<bool>(type: "boolean", nullable: false),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Languages = table.Column<bool>(type: "boolean", nullable: false),
                    Math = table.Column<bool>(type: "boolean", nullable: false),
                    Nature = table.Column<bool>(type: "boolean", nullable: false),
                    QuestionCount = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StudentId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TimeInMinutes = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamResults_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_StudentId_FinishAt",
                table: "ExamResults",
                columns: new[] { "StudentId", "FinishAt" });
        }
    }
}
