using Dapper;
using System.Data;

namespace Inkillay.Certificados.Web.Data.Repositories;

public class PagoRepository : IPagoRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public PagoRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> RegistrarPagoAsync(int idModulo, decimal monto, string referencia, 
        string formaPago = "Efectivo", string tipoPago = "Contado", string usuarioRegistro = "Sistema")
    {
        using var connection = _connectionFactory.CreateConnection();

        var resultado = await connection.QueryFirstOrDefaultAsync<bool>(
            "USP_Pagos_Registrar",
            new
            {
                IdModulo = idModulo,
                Monto = monto,
                Referencia = referencia,
                FormaPago = formaPago,
                TipoPago = tipoPago,
                UsuarioRegistro = usuarioRegistro
            },
            commandType: CommandType.StoredProcedure
        );

        return resultado;
    }
}
