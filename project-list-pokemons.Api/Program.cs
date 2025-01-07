using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text.Json.Serialization;
using Serilog;
using System.Text.Json;
using project_list_pokemons.Api.Data;
using project_list_pokemons.Api.Utils;
using project_list_pokemons.Api.Services;
using project_list_pokemons.Api.Repositories;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using project_list_pokemons.Api.Interfaces.Services;
using project_list_pokemons.Api.Interfaces.Repositories;
using project_list_pokemons.Api.Interfaces.Utils;

var builder = WebApplication.CreateBuilder(args);

// Configurar os arquivos de configuração
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory()) // Define o diretório base
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) 
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true) 
    .AddEnvironmentVariables();

// Configurar autenticação JWT
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // Porta para HTTP
});

// Configurar Serilog
builder.Host.UseSerilog((context, config) =>
{
    config
        .WriteTo.Console()
        .WriteTo.File("/app/logs/log.txt", rollingInterval: RollingInterval.Day)
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(context.Configuration);
});

// Configurar serviços da aplicação
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar System.Text.Json para evitar ciclos de referência
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Configurar banco de dados SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLiteConnection"), o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));


// Configurar Redis para Cache
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConfig = builder.Configuration.GetConnectionString("RedisConnection");
    return ConnectionMultiplexer.Connect(redisConfig ?? "");
});
builder.Services.AddSingleton<IRedisCacheHelper,RedisCacheHelper>();

// Registrar HttpClient
builder.Services.AddHttpClient<IHttpClientWrapper, HttpClientWrapper>();

// Registrar repositórios
builder.Services.AddScoped<IPokemonRepository,PokemonRepository>();
builder.Services.AddScoped<IMestrePokemonRepository,MestrePokemonRepository>();
builder.Services.AddScoped<ICapturaRepository,CapturaRepository>();

//// Registrar serviços
builder.Services.AddScoped<IPokemonService, PokemonService>();
builder.Services.AddScoped<IMestrePokemonService, MestrePokemonService>();
builder.Services.AddScoped<ICapturaService, CapturaService>();

//// Configurar inicialização do banco de dados em segundo plano
builder.Services.AddHostedService<DbInitializerService>();

// Configurar serviço de sincronização
builder.Services.AddHostedService<SyncService>();

// Configurar Swagger para documentação da API
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pokémon API",
        Version = "v1",
        Description = "API para gerenciar Pokémons, Mestres e Capturas.",
        Contact = new OpenApiContact
        {
            Name = "Hallison",
            Email = "halls510@hotmail.com",
            Url = new Uri("https://github.com/halls510/project-list-pokemons")
        }
    });

    // Configurações para incluir os comentários XML
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // Configuração de autenticação no Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. \r\n\r\n" +
                      "Insira 'Bearer' [espaço] e então o token na entrada abaixo.\r\n\r\n" +
                      "Exemplo: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Adicionar Middlewares
app.UseAuthentication();
app.UseAuthorization();

// Configurar Middlewares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pokémon API V1");
    });
}

app.MapControllers();

app.Run();