using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Inkillay.Certificados.Web.Data;

public sealed class DbConnectionFactory
{
    
    private const string DefaultProvider = "Microsoft.Data.SqlClient";

    private readonly string _connectionString;
    private readonly string _providerName;

    public DbConnectionFactory(IConfiguration configuration)
    {
       
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

       
        _providerName = configuration["Database:ProviderName"] ?? DefaultProvider;
    }

    public IDbConnection CreateConnection()
    {
        
        var providerFactory = DbProviderFactories.GetFactory(_providerName);

        var connection = providerFactory.CreateConnection()
            ?? throw new InvalidOperationException($"No se pudo crear la conexión para el proveedor '{_providerName}'.");

      
        connection.ConnectionString = _connectionString;
        return connection;
    }
}