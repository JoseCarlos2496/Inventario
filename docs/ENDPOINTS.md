# Inventario API — Endpoints

Convenciones: rutas en minúsculas `/api/inventario/...`, JSON camelCase, referencias por `uuid`. Ningún endpoint está protegido todavía.

## Formato de respuesta

Éxito:
```json
{ "success": true, "code": "ORD-S01", "message": "Orden de compra asignada.", "timestamp": "2026-10-14T15:04:05Z", "traceId": "00-abc...", "data": { } }
```
Error:
```json
{ "success": false, "code": "UBI-001", "message": "No existe la ubicación BOD-09.", "timestamp": "...", "traceId": "...", "path": "/api/inventario/compras/simular" }
```
Validación 400 · No encontrado 404 · Conflicto 409 · Inesperado 500 (`GEN-500`).

## Resumen

| Método | Ruta | Acción | Body | Código éxito |
|---|---|---|---|---|
| `POST` | `/api/inventario/ordenes` | ProcesarOrdenCompra | `{ ordenCompraId, lineas[{productoId?, sku?, cantidad}] }` | ORD-S01 |
| `GET` | `/api/inventario/ordenes/{ordenCompraId}` | ObtenerAsignacion | — | ORD-S02 |
| `POST` | `/api/inventario/compras/simular` | SimularCompra (hace de Compras) | ver abajo | COM-S01 (201) |
| `GET` | `/api/inventario/productos` | ListarProductos | — | PRO-S01 |
| `GET` | `/api/inventario/productos/{productoId}` | ObtenerProducto | — | PRO-S02 |
| `GET` | `/api/inventario/ubicaciones` | ListarUbicaciones | — | UBI-S01 |
| `POST` | `/api/inventario/ubicaciones` | CrearUbicacion | `{ codigo, nombre, tipo: Bodega\|Tienda\|Otro, prioridad }` | UBI-S02 (201) |
| `GET` | `/api/inventario/existencias` | ListarExistencias (total + desglose por ubicación) | — | EXI-S01 |
| `GET` | `/api/inventario/existencias/{productoId}` | ObtenerExistencia | — | EXI-S02 |
| `GET` | `/api/inventario/existencias/{productoId}/movimientos` | ListarMovimientos (kardex por ubicación) | — | MOV-S01 |
| `PUT` | `/api/inventario/existencias/{productoId}/stock-minimo` | DefinirStockMinimo | `{ stockMinimo }` | INV-S01 |
| `POST` | `/api/inventario/eventos` | RecibirEvento (webhook) | `SobreEvento` | EVT-S01 |
| `GET` | `/api/inventario/eventos/catalogo` | ObtenerCatalogo de eventos | — | EVT-S02 |
| `GET` | `/api/inventario/errores/catalogo` | ObtenerCatalogoErrores | — | ERR-S01 |
| `GET` | `/api/sistema/eventos-fallidos` | ListarEventosFallidos | — | SYS-S01 |
| `PUT` | `/api/sistema/eventos-fallidos/{id}/reintentar` | ReintentarEvento | — | SYS-S02 |

`POST /ordenes` responde **200 tanto si se asigna como si se rechaza**; ver `data.estado` (`Asignada` / `Rechazada`), `data.lineas[].recoger[]` (de dónde recoger) y `data.errores[]` (por qué se rechazó).

Al arrancar con la BD vacía se crean dos ubicaciones: `BOD-01` *Bodega principal* (prioridad 1) y `TDA-01` *Tienda* (prioridad 2).

### Simular compra

Hace las veces del microservicio de Compras: por cada línea crea el producto (o actualiza sus datos, buscando por `sku`) e ingresa la cantidad en la ubicación indicada. Un mismo `sku` puede venir en varias líneas para repartirlo entre ubicaciones. Si se envía `compraId`, reenviar la misma compra devuelve `COM-002` y no duplica el stock.

```json
{
  "compraId": "cccccccc-0000-0000-0000-000000000001",
  "lineas": [
    {
      "sku": "LAP-001", "nombre": "Laptop 14",
      "descripcion": "Laptop de oficina", "marca": "Lenovo", "modelo": "ThinkPad E14",
      "categoria": "Cómputo", "unidadMedida": "unidad",
      "caracteristicas": { "RAM": "16 GB", "SSD": "512 GB" },
      "codigoUbicacion": "BOD-01", "cantidad": 4
    },
    { "sku": "LAP-001", "nombre": "Laptop 14", "codigoUbicacion": "TDA-01", "cantidad": 1 }
  ]
}
```

Los campos opcionales que lleguen en `null` conservan el valor anterior; las `caracteristicas` se agregan o reemplazan por clave.

## Códigos de error

Generados desde `CatalogoErrores` (`GET /api/inventario/errores/catalogo` devuelve esta misma tabla). Los de tipo **Rechazo** no son errores HTTP: viajan dentro de `inventario.orden.rechazada` y en `data.errores[]` de `POST /ordenes`.

| Código | Tipo | HTTP | Significado |
|---|---|---|---|
| VAL-000 | Validación | 400 | Body con formato inválido (`errors[]`) |
| VAL-001 | Validación | 400 | Cantidad ≤ 0 |
| VAL-002 | Validación | 400 | SKU vacío o > 50 caracteres |
| VAL-003 | Validación | 400 | Nombre vacío o > 200 caracteres |
| VAL-004 | Validación | 400 | Dato o característica del producto inválido (clave vacía o texto demasiado largo) |
| PRO-001 | No encontrado | 404 | El producto no existe |
| UBI-001 | No encontrado | 404 | La ubicación no existe |
| UBI-002 | Conflicto | 409 | Ya existe una ubicación con ese código |
| UBI-003 | Validación | 400 | Prioridad negativa |
| EXI-001 | Validación | 400 | La existencia requiere producto y ubicación |
| EXI-002 | Conflicto | 409 | La ubicación no tiene unidades suficientes para descontar |
| COM-001 | Validación | 400 | Compra sin líneas |
| COM-002 | Conflicto | 409 | Esa compra (`compraId`) ya fue registrada |
| INV-001 | Validación | 400 | Stock mínimo negativo |
| ORD-001 | Validación | 400 | La orden de compra no trae `ordenCompraId` |
| ORD-002 | No encontrado | 404 | Inventario no ha procesado esa orden de compra |
| ORD-100 | Rechazo | — | La orden no tiene líneas |
| ORD-101 | Rechazo | — | El producto no está registrado en Inventario |
| ORD-102 | Rechazo | — | La suma de todas las ubicaciones no alcanza |
| ORD-103 | Rechazo | — | El producto no tiene unidades en ninguna ubicación |
| ORD-104 | Rechazo | — | La línea pide cantidad ≤ 0 |
| ORD-105 | Rechazo | — | La línea no indica `productoId` ni `sku` |
| EVT-001 | Validación | 400 | Evento (tipo/versión) que Inventario no consume |
| EVT-002 | Validación | 400 | Payload del evento inválido |
| SYS-001 | No encontrado | 404 | Evento de Outbox inexistente |
| SYS-002 | Conflicto | 409 | El evento no está fallido |
| GEN-409 | Conflicto | 409 | Conflicto de concurrencia; reintentar |
| GEN-500 | Sistema | 500 | Error inesperado |
