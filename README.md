# Inventario

Microservicio de **Inventario** (.NET 10, DDD + eventos). Recibe de **Recepción** la orden de compra, valida y ubica el stock en cada ubicación (bodega, tienda, ...), lo descuenta y le indica a **Preparación** qué recoger y dónde; Preparación sigue con **Despacho**. Si la orden no se puede surtir completa, responde a Recepción con el porqué (catálogo de errores).

Flujo lineal: `Recepción → Inventario → Preparación → Despacho`.

```
dotnet build Inventario.slnx
dotnet run --project Api/Inventario.Api --launch-profile http    # http://localhost:5180
```

Arranca sin dependencias (BD en memoria, eventos publicados al log). Peticiones de ejemplo en [Api/Inventario.Api/Inventario.Api.http](Api/Inventario.Api/Inventario.Api.http).

Documentación:
- [docs/FLUJO_Y_EVENTOS.md](docs/FLUJO_Y_EVENTOS.md) — contrato de eventos para los otros equipos
- [docs/ARQUITECTURA.md](docs/ARQUITECTURA.md) — agregados, capas, patrones y configuración
- [docs/ENDPOINTS.md](docs/ENDPOINTS.md) — API HTTP y códigos de error
