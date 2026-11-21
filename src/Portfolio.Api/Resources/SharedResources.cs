namespace Portfolio.Api.Resources;

public class SharedResources
{
    // Classe vazia usada apenas como referência para o IStringLocalizer
}


/**

using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// 1. Adiciona os serviços de localização
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// 2. Configura os idiomas suportados
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("pt-BR"),
        new CultureInfo("en-US")
    };

    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

// 3. Habilita o middleware ANTES de mapear os controllers
app.UseRequestLocalization();

 */