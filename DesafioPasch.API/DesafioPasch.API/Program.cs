using DesafioPasch.Application.Contracts;
using DesafioPasch.Application.Services;
using DesafioPasch.Data.Repositories;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DesafioPasch API",
        Version = "v1",
        Description = "API para listagem de títulos em atraso com cálculo de multa e juros conforme o desafio."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

builder.Services.Configure<DataFilesOptions>(builder.Configuration.GetSection("DataFiles"));

builder.Services.AddSingleton<ITituloRepository, JsonTituloRepository>();

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<TitulosEmAtrasoService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapControllers();
app.Run();

sealed class SystemClock : IClock
{
    public DateOnly Hoje() => DateOnly.FromDateTime(DateTime.Now);
}

/// <summary>
/// Converter robusto para DateOnly no formato yyyy-MM-dd.
/// </summary>
sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateOnly.ParseExact(reader.GetString()!, Format);

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format));
}