namespace Portfolio.Api.Features.ExamResults;

public static class ExamResultsModule
{
    public static IServiceCollection AddExamResultsFeature(this IServiceCollection services)
    {
        services.AddScoped<IExamResultService, ExamResultService>();
        services.AddScoped<ITeacherStudentService, TeacherStudentService>();
        services.AddHttpClient<IEnemQuestionsClient, EnemQuestionsClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.enem.dev/v1/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        return services;
    }
}
