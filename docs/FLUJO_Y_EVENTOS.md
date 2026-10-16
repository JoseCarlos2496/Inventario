# Inventario — Flujo y contrato de eventos

Documento para los equipos de **Recepción**, **Preparación** y **Despacho**: qué eventos consume Inventario, cuáles publica y cómo deben viajar. Complementa [ARQUITECTURA.md](ARQUITECTURA.md) y [ENDPOINTS.md](ENDPOINTS.md).

## 1. Dónde está Inventario en el flujo

El flujo es **lineal**: cada equipo recibe el trabajo del anterior y se lo pasa al siguiente. Ningún paso le devuelve nada a Inventario.

```mermaid
flowchart LR
  REC["Recepción"] -->|"recepcion.orden-compra.recibida"| INV["Inventario"]
  INV -->|"inventario.orden.asignada"| PRE["Preparación"]
  PRE -->|"evento de Preparación"| DES["Despacho"]
  INV -.->|"inventario.orden.rechazada"| REC
```

Si tu visor no muestra diagramas Mermaid, este es el mismo flujo:

```
Recepción ──recepcion.orden-compra.recibida──▶ Inventario ──inventario.orden.asignada──▶ Preparación ──▶ Despacho
    ▲                                              │
    └──────────inventario.orden.rechazada──────────┘   (solo si no se puede surtir la orden)
```

1. **Recepción** recibe la orden de compra y se la pasa a Inventario (`recepcion.orden-compra.recibida`).
2. **Inventario** procesa la lista:
   - **Valida la orden completa**: que cada producto exista y que la suma de todas sus ubicaciones alcance.
   - **Ubica** las unidades por ubicación (bodega, tienda, ...) y **descuenta** el stock en ese momento.
   - Avisa a **Preparación** qué recoger y **en qué ubicación** (`inventario.orden.asignada`).
   - Si algo no alcanza, **no descuenta nada** y responde a Recepción con el porqué (`inventario.orden.rechazada`, con códigos del [catálogo de errores](#4-catálogo-de-errores-de-rechazo)).
3. **Preparación** recoge en cada ubicación y avisa a **Despacho**. Ese evento es entre ellos: Inventario no participa.

Ejemplo: la orden pide 5 laptops; hay 4 en *Bodega principal* y 1 en *Tienda*. Inventario descuenta las 5 y le dice a Preparación: «4 en BOD-01 y 1 en TDA-01».

**Orden de recogida:** primero las ubicaciones con menor `prioridad` (por defecto Bodega = 1, Tienda = 2). Con la misma prioridad, la que tenga más unidades, para partir el pedido lo menos posible.

**De dónde sale el stock:** debería entrar por **Compras**. Como ese microservicio no existe todavía, `POST /api/inventario/compras/simular` crea el producto con sus características y le ingresa stock en la ubicación que se indique (ver [ENDPOINTS.md](ENDPOINTS.md)).

> **Pendiente de acordar con los otros equipos:** el nombre exacto de `recepcion.orden-compra.recibida` y si Recepción identifica los productos por `productoId` o por `sku` (Inventario acepta los dos). Si cambia el nombre, solo se ajusta el atributo `[EventoIntegracion]` del contrato.

## 2. El sobre (lo que viaja)

Todo evento viaja dentro de un `SobreEvento` en JSON camelCase, igual por RabbitMQ o por HTTP:

```json
{
  "eventoId": "eeeeeeee-0000-0000-0000-000000000001",
  "tipo": "recepcion.orden-compra.recibida",
  "version": 1,
  "ocurridoEn": "2026-10-14T10:00:00Z",
  "origen": "recepcion",
  "payload": { }
}
```

| Campo | Regla |
|---|---|
| `eventoId` | Único por evento. Inventario lo usa para **no aplicar dos veces** el mismo evento (entrega al menos una vez). Reenviar el mismo evento debe reutilizar el mismo `eventoId`. |
| `tipo` | Nombre estable `contexto.entidad.accion`. En RabbitMQ también es la **routing key**. |
| `version` | Versión del contrato del `payload`. |
| `payload` | El evento en sí (tablas de abajo). Identificadores siempre `uuid`. |

## 3. Eventos

### Inventario CONSUME

| Nombre estable (v1) | Lo emite | Payload | Efecto en Inventario |
|---|---|---|---|
| `recepcion.orden-compra.recibida` | Recepción | `ordenCompraId, lineas[{productoId?, sku?, cantidad}]` | Valida, ubica y descuenta todo o nada. Publica `inventario.orden.asignada` o `inventario.orden.rechazada`. Una por `ordenCompraId`: repetirla devuelve el mismo resultado y **no vuelve a descontar**. |

Cada línea trae `productoId` **o** `sku` (si vienen los dos, manda `productoId`). Si el mismo producto aparece en varias líneas, se suma.

```json
"payload": {
  "ordenCompraId": "aaaaaaaa-0000-0000-0000-000000000001",
  "lineas": [ { "sku": "LAP-001", "cantidad": 5 }, { "sku": "MOU-001", "cantidad": 1 } ]
}
```

### Inventario PUBLICA

| Nombre estable (v1) | Cuándo | Payload | Interesado |
|---|---|---|---|
| `inventario.orden.asignada` | Hubo stock para toda la orden; **ya se descontó** | `asignacionId, ordenCompraId, lineas[{productoId, sku, nombre, cantidad, recoger[{ubicacionId, codigoUbicacion, nombreUbicacion, tipoUbicacion, cantidad}]}]` | **Preparación** (recoge en cada ubicación) |
| `inventario.orden.rechazada` | Faltó algo; no se descontó nada | `asignacionId, ordenCompraId, motivo, errores[{codigo, mensaje, productoId?, sku?, cantidadSolicitada, cantidadDisponible}]` | **Recepción** |
| `inventario.stock.ingresado` | Entró stock (compra simulada) | `productoId, ubicacionId, cantidadIngresada, cantidadEnUbicacion, compraId` | Quien necesite saber que hay stock nuevo |
| `inventario.stock.bajo-minimo` | El total del producto (todas las ubicaciones) cae por debajo del mínimo; solo al cruzar el umbral | `productoId, cantidadDisponible, stockMinimo` | Compras / alertas |

Ejemplo de `inventario.orden.asignada` (5 laptops, 4 en bodega y 1 en tienda):

```json
"payload": {
  "asignacionId": "…",
  "ordenCompraId": "aaaaaaaa-0000-0000-0000-000000000001",
  "lineas": [{
    "productoId": "…", "sku": "LAP-001", "nombre": "Laptop 14", "cantidad": 5,
    "recoger": [
      { "ubicacionId": "…", "codigoUbicacion": "BOD-01", "nombreUbicacion": "Bodega principal", "tipoUbicacion": "Bodega", "cantidad": 4 },
      { "ubicacionId": "…", "codigoUbicacion": "TDA-01", "nombreUbicacion": "Tienda", "tipoUbicacion": "Tienda", "cantidad": 1 }
    ]
  }]
}
```

Ejemplo de `inventario.orden.rechazada` (Inventario reporta **todos** los problemas de la orden, no solo el primero):

```json
"payload": {
  "asignacionId": "…",
  "ordenCompraId": "aaaaaaaa-0000-0000-0000-000000000002",
  "motivo": "La orden tiene 2 problemas; ver errores.",
  "errores": [
    { "codigo": "ORD-102", "mensaje": "Laptop 14 (LAP-001): se piden 6 y solo hay 5 (Bodega principal: 4, Tienda: 1).", "productoId": "…", "sku": "LAP-001", "cantidadSolicitada": 6, "cantidadDisponible": 5 },
    { "codigo": "ORD-101", "mensaje": "El producto con SKU NOEXISTE no está registrado en Inventario.", "sku": "NOEXISTE", "cantidadSolicitada": 1, "cantidadDisponible": 0 }
  ]
}
```

`GET /api/inventario/eventos/catalogo` devuelve esta lista generada desde el código.

## 4. Catálogo de errores de rechazo

Los motivos que viajan en `inventario.orden.rechazada`. El catálogo completo (incluye los errores HTTP) está en `GET /api/inventario/errores/catalogo` y en [ENDPOINTS.md](ENDPOINTS.md#códigos-de-error); ambos salen de `CatalogoErrores` en el código.

| Código | Significado |
|---|---|
| ORD-100 | La orden de compra no tiene líneas |
| ORD-101 | El producto no está registrado en Inventario |
| ORD-102 | La suma de todas las ubicaciones no alcanza la cantidad pedida (el mensaje trae el desglose por ubicación) |
| ORD-103 | El producto no tiene unidades en ninguna ubicación |
| ORD-104 | La línea pide una cantidad ≤ 0 |
| ORD-105 | La línea no indica `productoId` ni `sku` |

Una orden sin `ordenCompraId` no se puede correlacionar, así que no se rechaza con evento: termina en fallidos con `ORD-001`.

## 5. Transporte

**RabbitMQ (recomendado):**
- Exchange **topic** compartido `taller.eventos` (durable). Routing key = `tipo`.
- Inventario declara su cola `inventario.eventos` y la enlaza a `recepcion.orden-compra.recibida`.
- Cada equipo declara **su propia cola** y la enlaza a lo que le interesa (Preparación: `inventario.orden.asignada`; Recepción: `inventario.orden.rechazada`).
- Propiedades del mensaje que pone Inventario: `messageId = eventoId`, `type = tipo.vN`, `contentType = application/json`, persistente.
- Lo que Inventario no puede procesar termina en `inventario.eventos.fallidos` (dead letter).

**HTTP (alternativa / pruebas):** `POST /api/inventario/eventos` con el mismo sobre, o `POST /api/inventario/ordenes` con el payload directo. Misma idempotencia.

Para probar sin broker: `"Mensajeria:Proveedor": "Log"` (por defecto). Los eventos publicados se escriben en el log.

### Broker en la nube (plan gratuito, p. ej. CloudAMQP)

Todos los equipos deben usar **la misma instancia y el mismo exchange** `taller.eventos`.

1. Crear la instancia y copiar su URL `amqps://usuario:clave@host/vhost` (TLS).
2. Configurar Inventario por variables de entorno, **sin subir la URL al repositorio**:
   ```
   Mensajeria__Proveedor=RabbitMq
   Mensajeria__RabbitMq__Uri=amqps://usuario:clave@host/vhost
   ```
   (en local también sirve `dotnet user-secrets`). Si `Uri` está definida, se ignoran HostName/Port/UserName/Password.
3. Inventario declara el exchange y sus colas al arrancar; no hay que crearlos a mano.

Consideraciones de los planes gratuitos:
- **Conexiones limitadas:** Inventario abre solo 2 (una para publicar, otra para consumir). Cada equipo debería hacer lo mismo y no abrir una conexión por mensaje.
- **Mensajes por mes y tamaño de cola limitados:** no usarlo para pruebas de carga; la cola de fallidos `inventario.eventos.fallidos` debe vaciarse de vez en cuando.
- **Cortes por inactividad o mantenimiento:** Inventario envía heartbeat cada 30 s, el consumidor se reconecta solo (cada 10 s) y la Outbox conserva lo pendiente; no se pierden eventos si el broker se cae, solo se retrasan. Si la caída supera los 3 intentos, el evento queda en `GET /api/sistema/eventos-fallidos` y se reencola con `PUT .../reintentar`.

## 6. Garantías

- **Todo o nada:** si una sola línea no se puede surtir, no se descuenta ninguna.
- **Outbox:** los eventos que publica Inventario se guardan en la tabla `outbox` en la **misma transacción** que el descuento de stock; si la transacción falla, no sale ningún evento y no se descuenta nada. Un proceso en segundo plano los publica cada ~2 s, en orden.
- **Reintentos (máx. 3, esperas 5 s y 10 s)** tanto al publicar (Outbox) como al consumir. Al agotarse: `WARNING "EVENTO FALLIDO"` en el log y, en la Outbox, visible en `GET /api/sistema/eventos-fallidos` con opción de reintentar.
- **Al menos una vez:** los consumidores (incluido Inventario) deben ser idempotentes; Inventario lo es por `eventoId` (tabla `eventos_recibidos`) y por `ordenCompraId`.
- **Concurrencia:** dos órdenes simultáneas no pueden descontar las mismas unidades (token de versión por existencia); la que pierde se reintenta y se evalúa contra el stock ya descontado.
- Consistencia **eventual**: Preparación recibe la asignación pocos segundos después de que Recepción envía la orden.

## 7. Versionado de contratos

Los contratos viven en `Inventario.Contratos` (sin dependencias; se puede compartir como paquete NuGet).
- Dentro de una versión solo se agregan **campos opcionales**.
- Cambio incompatible = clase nueva con `version + 1`; se publican ambas durante la transición.
- Nunca reutilizar un nombre estable con otro significado.
