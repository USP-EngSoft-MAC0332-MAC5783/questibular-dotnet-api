namespace Portfolio.DataAccess.Mapping;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        // ── USER ───────────────────────────────────────────────────────
        TypeAdapterConfig<UserDbModel, User>.NewConfig()
            .MapWith(src => User.Reconstitute(
                src.Id, src.Email, src.Name, src.PasswordHash,
                src.Roles, src.TwoFactorEnabled, src.TwoFactorSecret,
                src.TwoFactorRecoveryCodes, src.AccessFailedCount,
                src.LockoutEnd, src.LastLoginAt, src.CreatedAt,
                src.EmailConfirmed, src.TwoFactorEnabledAt
            ));

        // Save
        TypeAdapterConfig<User, UserDbModel>.NewConfig()
            .Map(dest => dest.Roles, src => src.Roles.ToList())
            .Map(dest => dest.TwoFactorRecoveryCodes, src => src.TwoFactorRecoveryCodes.ToList());

        // ── REFRESH TOKEN ──────────────────────────────────────────────
        // Save
        TypeAdapterConfig<RefreshToken, RefreshTokenDbModel>.NewConfig();

        // Read - Factory Method
        TypeAdapterConfig<RefreshTokenDbModel, RefreshToken>.NewConfig()
            .MapWith(src => RefreshToken.Reconstitute(
                src.Id, src.UserId, src.TokenHash, src.ExpiresAt,
                src.RevokedAt, src.ReplacedByTokenHash, src.CreatedByIp,
                src.RevokedByIp, src.CreatedAt
            ));

        // ── EXAM ───────────────────────────────────────────────────────
        TypeAdapterConfig<ExamDbModel, Exam>.NewConfig()
            .MapWith(src => Exam.Reconstitute(
                src.Id, 
                src.Title, 
                src.Year, 
                src.QuestionCountHumanities, 
                src.QuestionCountNature, 
                src.QuestionCountLanguages, 
                src.QuestionCountMath,
                src.Disciplines, 
                src.Languages,
                src.IsSynced, 
                src.StartCount, 
                src.FinishCount, 
                src.TotalQuestionsFinished,
                src.LastStartedAt, 
                src.LastFinishedAt, 
                src.CreatedAt
            ));

        // Exam Attempt
        TypeAdapterConfig<ExamAttemptDbModel, ExamAttempt>.NewConfig()
            .MapWith(src => ExamAttempt.Reconstitute(
                src.Id, 
                src.ExamId, 
                src.UserId, 
                src.IsAnonymous,
                src.AnonymousToken,
                src.StartedAt,
                src.PlannedFinishAt,
                src.FinishedAt,
                src.ExpiresAt,
                src.TimeInMinutes,
                src.QuestionCount,
                src.SelectedLanguage,
                src.IncludeHumanities,
                src.IncludeNature,
                src.IncludeLanguages,
                src.IncludeMath,
                src.ServedQuestions.Select(q => new AttemptQuestion(q.Index, q.Discipline, q.Language)).ToList(),
                src.Answers.Select(a => QuestionAnswer.Create(a.Index, a.CorrectAlternative, a.SelectedAlternative)).ToList(),
                src.CreatedAt
            ));

        TypeAdapterConfig<Exam, ExamDbModel>.NewConfig()
            .Map(dest => dest.Disciplines, src => src.Disciplines.ToList())
            .Map(dest => dest.Languages, src => src.Languages.ToList());

        TypeAdapterConfig<ExamAttempt, ExamAttemptDbModel>.NewConfig()
            .Map(dest => dest.ServedQuestions, src => src.ServedQuestions.Select(q => new AttemptQuestionDbModel { 
                Index = q.Index, 
                Discipline = q.Discipline, 
                Language = q.Language 
            }).ToList())
            .Map(dest => dest.Answers, src => src.Answers.Select(a => new QuestionAnswerDbModel { 
                Index = a.Index, 
                CorrectAlternative = a.CorrectAlternative, 
                SelectedAlternative = a.SelectedAlternative 
            }).ToList());
    }

    /// <summary>
    /// Automatically runs when the assembly of the project is loaded,
    /// guaranteeing that Mapster is configured in any process
    /// (API, tests, CLI tools, CI jobs).
    /// </summary>
    [ModuleInitializer]
    internal static void Initialize() => RegisterMappings();
}