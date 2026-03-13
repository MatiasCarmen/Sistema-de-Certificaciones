using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace Inkillay.Certificados.Web.Services;

public interface IAuditoriaService
{
    Task RegistrarAsync(int? idUsuario, string accion, string modulo, string detalles, string ip);
    Task<List<(string accion, string detalles, DateTime fecha, string ip)>> ObtenerUltimosAsync(int cantidad = 5);

    /// <summary>
    /// Obtiene el nombre del usuario actual desde los Claims para campos de auditoría
    /// </summary>
    string? ObtenerUsuarioActual(ClaimsPrincipal? user);
}

public class AuditoriaService : IAuditoriaService
{
    private readonly string _connectionString;

    public AuditoriaService(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Obtiene el nombre del usuario actual desde los Claims
    /// Este método se usa para capturar UsuarioRegistro y UsuarioModifica
    /// </summary>
    public string? ObtenerUsuarioActual(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        // Intenta obtener el nombre del usuario desde los Claims
        return user.FindFirst(ClaimTypes.Name)?.Value 
            ?? user.FindFirst(ClaimTypes.Email)?.Value 
            ?? "Sistema";
    }

    public async Task RegistrarAsync(int? idUsuario, string accion, string modulo, string detalles, string ip)
    {
        try
        {
            using (var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    INSERT INTO Auditoria (IdUsuario, Accion, Modulo, Detalles, DireccionIp, FechaHora, Exitoso)
                    VALUES (@idUsuario, @accion, @modulo, @detalles, @ip, GETDATE(), 1)";

                using (var command = new Microsoft.Data.SqlClient.SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@idUsuario", idUsuario ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@accion", accion);
                    command.Parameters.AddWithValue("@modulo", modulo);
                    command.Parameters.AddWithValue("@detalles", detalles);
                    command.Parameters.AddWithValue("@ip", ip ?? "DESCONOCIDA");

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registrando auditoría: {ex.Message}");
        }
    }

    public async Task<List<(string accion, string detalles, DateTime fecha, string ip)>> ObtenerUltimosAsync(int cantidad = 5)
    {
        var registros = new List<(string, string, DateTime, string)>();

        try
        {
            using (var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = $@"
                    SELECT TOP {cantidad} Accion, Detalles, FechaHora, DireccionIp
                    FROM Auditoria
                    ORDER BY FechaHora DESC";

                using (var command = new Microsoft.Data.SqlClient.SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            registros.Add((
                                reader["Accion"].ToString() ?? "",
                                reader["Detalles"].ToString() ?? "",
                                (DateTime)reader["FechaHora"],
                                reader["DireccionIp"].ToString() ?? ""
                            ));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo auditoría: {ex.Message}");
        }

        return registros;
    }
}
