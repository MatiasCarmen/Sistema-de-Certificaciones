namespace Inkillay.Certificados.Web.Utils;

/// <summary>
/// Enum para el estado de registro (Activo/Inactivo) según estándar corporativo
/// </summary>
public enum EstadoRegistro
{
    /// <summary>Activo - Valor: 'A'</summary>
    Activo = 'A',
    
    /// <summary>Inactivo - Valor: 'I'</summary>
    Inactivo = 'I'
}

/// <summary>
/// Enum para el estado de matrícula (valores 1-6)
/// </summary>
public enum EstadoModulo
{
    /// <summary>1: Matrícula registrada pero no ha iniciado el curso</summary>
    Registrada = 1,
    
    /// <summary>2: El curso está en progreso</summary>
    EnCurso = 2,
    
    /// <summary>3: El curso ha finalizado pero aún no se ha evaluado</summary>
    Completada = 3,
    
    /// <summary>4: El alumno aprobó el curso</summary>
    Aprobada = 4,
    
    /// <summary>5: El alumno reprobó el curso</summary>
    Reprobada = 5,
    
    /// <summary>6: Matrícula cancelada por el alumno o por administración</summary>
    Cancelada = 6
}

/// <summary>
/// Enum para la modalidad de cursado
/// </summary>
public enum Modalidad
{
    /// <summary>P: Presencial</summary>
    Presencial = 'P',
    
    /// <summary>R: Remoto (online)</summary>
    Remoto = 'R',
    
    /// <summary>H: Híbrido (combinación de presencial y remoto)</summary>
    Hibrido = 'H'
}

/// <summary>
/// Enum para las formas de pago disponibles
/// </summary>
public enum FormaPago
{
    Efectivo,
    Tarjeta,
    Transferencia,
    Yape,
    Plin
}

/// <summary>
/// Enum para el tipo de pago
/// </summary>
public enum TipoPago
{
    Contado,
    Fraccionado,
    Cuota
}

/// <summary>
/// Clase de ayuda para convertir entre enums y valores de base de datos
/// </summary>
public static class DiccionarioHelper
{
    /// <summary>
    /// Convierte un char a EstadoRegistro
    /// </summary>
    public static EstadoRegistro ToEstadoRegistro(char estado)
    {
        return estado switch
        {
            'A' => EstadoRegistro.Activo,
            'I' => EstadoRegistro.Inactivo,
            _ => EstadoRegistro.Inactivo
        };
    }
    
    /// <summary>
    /// Convierte un string a EstadoRegistro
    /// </summary>
    public static EstadoRegistro ToEstadoRegistro(string estado)
    {
        if (string.IsNullOrEmpty(estado)) return EstadoRegistro.Inactivo;
        return ToEstadoRegistro(estado[0]);
    }
    
    /// <summary>
    /// Convierte EstadoRegistro a char para la base de datos
    /// </summary>
    public static char ToChar(this EstadoRegistro estado)
    {
        return (char)estado;
    }
    
    /// <summary>
    /// Convierte Modalidad a char para la base de datos
    /// </summary>
    public static char ToChar(this Modalidad modalidad)
    {
        return (char)modalidad;
    }
    
    /// <summary>
    /// Convierte un char a Modalidad
    /// </summary>
    public static Modalidad ToModalidad(char modalidad)
    {
        return modalidad switch
        {
            'P' => Modalidad.Presencial,
            'R' => Modalidad.Remoto,
            'H' => Modalidad.Hibrido,
            _ => Modalidad.Presencial
        };
    }
    
    /// <summary>
    /// Convierte FormaPago enum a string
    /// </summary>
    public static string ToFormaPagoString(this FormaPago formaPago)
    {
        return formaPago.ToString();
    }
    
    /// <summary>
    /// Convierte string a FormaPago enum
    /// </summary>
    public static FormaPago ToFormaPago(string formaPago)
    {
        return Enum.TryParse<FormaPago>(formaPago, out var result) 
            ? result 
            : FormaPago.Efectivo;
    }
}
