# Contexto del Proyecto: LLEGÓ EN 5' (Runner 2D)

Ver `README.md` para el concepto del juego.

## Stack y Versión
- **Motor:** Unity 6.6 (6000.6.5f1), 2D Core, Universal Render Pipeline 2D
- **Lenguaje:** C# (.NET / CoreCLR)
- **Input:** New Input System (`UnityEngine.InputSystem`)
- **Física:** Physics2D (`Rigidbody2D`, `Collider2D`)

## Reglas de Arquitectura y Código
1. **Encapsulamiento:** Nunca uses variables `public` para referencias del Inspector; usa `[SerializeField] private`.
2. **Rendimiento:**
   - Prohibido llamar a `GetComponent()`, `Find()`, o instanciar objetos dentro de `Update()`.
   - Almacena referencias en `Awake()` o `Start()`.
   - Toda interacción con `Rigidbody2D` (fuerzas, velocidad) debe ejecutarse en `FixedUpdate()`.
3. **Unity 6.6 Peculiaridades:**
   - **Fast Enter Play Mode:** En 6.6 el modo por defecto no recarga dominio completo. Si usas eventos estáticos o estados globales, asegúrate de limpiarlos en `OnDisable()` / `OnDestroy()` o usa `[RuntimeInitializeOnLoadMethod]`.
   - No uses wrappers para serializar diccionarios: Unity no serializa `Dictionary<TKey, TValue>` de forma nativa. Usa listas serializables de pares clave/valor, o un `ScriptableObject`.
4. **Mecánicas del Juego (Runner 2D):**
   - El movimiento horizontal es constante o acelerado progresivamente.
   - Detección de suelo mediante `Physics2D.OverlapCircle` o `BoxCast`, no solo colisiones planas.
   - Las animaciones controlan el estado (Running, Vaulting, Sliding, Stumble).

## Archivos excluidos
- `.claude/settings.json` (`permissions.deny`) bloquea lectura y escritura de `Library/`, `Temp/`, `Obj/`, builds, `Logs/`, `UserSettings/`, `MemoryCaptures/` y los `.csproj`/`.sln`/`.unityproj` generados.
- Escenas, prefabs, assets y `.meta` **no** están excluidos: Claude los necesita para resolver referencias por GUID.
