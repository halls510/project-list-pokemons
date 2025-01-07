using Microsoft.EntityFrameworkCore;
using project_list_pokemons.Api.Data;

namespace project_list_pokemons.Api.Services
{
    public class DbInitializerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DbInitializerService> _logger;
        private static bool _isInitialized = false; // Controle de execução única
        private readonly string _databasePath;
        private static readonly object _lock = new();

        public DbInitializerService(IServiceProvider serviceProvider, ILogger<DbInitializerService> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // Obtém o caminho do banco SQLite
            _databasePath = configuration.GetConnectionString("SQLiteConnection")?.Split('=')[1] ?? "app.db";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {          
            lock (_lock) // Garante execução única mesmo em cenários multi-thread
            {
                if (_isInitialized)
                {
                    _logger.LogWarning("Inicialização do banco de dados já foi executada. Ignorando nova tentativa.");
                    return;
                }

                _isInitialized = true;
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                // Verificar se o banco já existe
                if (File.Exists(_databasePath))
                {
                    _logger.LogInformation("Banco de dados já existe em: {Path}", _databasePath);

                    // Verificar se a variável de ambiente permite excluir e recriar o banco
                    var resetDatabase = Environment.GetEnvironmentVariable("RESET_DATABASE_SQLITE")?.ToLower() == "true";

                    if (resetDatabase)
                    {
                        _logger.LogWarning("RESET_DATABASE_SQLITE está ativado. Excluindo o banco existente...");
                        File.Delete(_databasePath);
                        _logger.LogInformation("Banco de dados excluído.");
                    }
                    else
                    {
                        _logger.LogInformation("RESET_DATABASE_SQLITE não está ativado. Mantendo o banco existente.");
                        _isInitialized = true;
                        return; // Não recriar banco
                    }
                }

                // Criar o banco de dados
                _logger.LogInformation("Criando o banco de dados...");
                await context.Database.EnsureCreatedAsync(stoppingToken);
                _logger.LogInformation("Banco de dados criado com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar o banco de dados.");
            }

        }
    }
}