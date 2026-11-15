// =============================================================================
// ExamModule
// Registro DI da feature de Exams. Chamar em Program.cs:
//     builder.Services.AddExamFeature();
// =============================================================================

using Microsoft.Extensions.DependencyInjection;

namespace Portfolio.Api.Features.Exams;

public static class ExamModule
{
    public static IServiceCollection AddExamFeature(this IServiceCollection services)
    {
        services.AddScoped<IExamFacade, ExamFacade>();
        services.AddScoped<IExamSyncService, ExamSyncService>();
        return services;
    }
}