namespace Portfolio.Api.Infrastructure.EnemApi;

// =============================================================================
// EnemApiModule
// Configure the HttpClient typed (base address, headers, timeout) and register the
// builder/factory/dispatcher.
// =============================================================================
public static class EnemApiModule
{
    public static IServiceCollection AddEnemApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EnemApiOptions>(opts =>
        {
            opts.BaseUrl  = configuration["ENEM_API_URL"]
                            ?? throw new InvalidOperationException("ENEM_API_URL not configured.");
            opts.Version  = configuration["ENEM_API_VERSION"] ?? "v1";

            opts.AllExamsEndpoint         = configuration["ENEM_API_EXAM_LIST_ENDPOINT"]          ?? "";
            opts.ExamEndpoint             = configuration["ENEM_API_EXAM_ENDPOINT"]               ?? "";
            opts.ExamQuestionListEndpoint = configuration["ENEM_API_EXAM_QUESTION_LIST_ENDPOINT"] ?? "";
            opts.ExamQuestionEndpoint     = configuration["ENEM_API_EXAM_QUESTION_ENDPOINT"]      ?? "";
        });

        services.AddHttpClient<IEnemApiDispatcher, EnemApiDispatcher>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<EnemApiOptions>>().Value;
            client.BaseAddress = new Uri($"{opts.BaseUrl.TrimEnd('/')}/{opts.Version.Trim('/')}/");
            client.Timeout     = opts.Timeout;
        });

        services.AddSingleton<IEnemApiRequestFactory, EnemApiRequestFactory>();
        services.AddScoped<IEnemApiClient, EnemApiClient>();

        return services;
    }
}