namespace Inventario.Api.Respuestas
{
    /// <summary>
    /// Formato único de respuesta (éxito y error).
    /// </summary>
    public sealed record ApiResponse(
        bool Success,
        string Code,
        string Message,
        DateTime Timestamp,
        string TraceId,
        object? Data = null,
        IReadOnlyList<ErrorCampo>? Errors = null,
        string? Path = null);

    public sealed record ErrorCampo(string Field, string Message);
}
