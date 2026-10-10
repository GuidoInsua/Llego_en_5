# Contexto del Proyecto: LLEGÓ EN 5' (Runner 2D)

Ver `README.md` para el concepto, los objetivos del proyecto y el recorrido del Nivel 1.

## Estado Actual (prototipo)

Hoy existe en Unity:
- Personaje que corre a velocidad constante y salta (`Player_controller`).
- Suelo generado en tramos delante del jugador y borrado detrás (`Ground_spawner`).
- Un test PlayMode del spawner.

Todavía no existe: deslizarse, tropiezo, monedas, patada, subte y tienda, reloj, jefe, historieta y arte final.

**Objetivo inmediato (README, corto plazo):** núcleo jugable en un tramo de prueba: correr, saltar, deslizarse y tropiezo.

## Stack y Versión
- **Motor:** Unity 6.6 (6000.6.5f1), 2D Core, Universal Render Pipeline 2D
- **Lenguaje:** C# (.NET / CoreCLR)
- **Input:** New Input System (`UnityEngine.InputSystem`)
- **Física:** Physics2D (`Rigidbody2D`, `Collider2D`)

## Estructura
- `Assets/Scripts/`: código del juego. Assembly `Scripts` (`Scripts.asmdef`, referencia `Unity.InputSystem`).
- `Assets/Tests/`: tests PlayMode. Assembly `Tests` (`Tests.asmdef`), referencia `Scripts` y solo compila con `UNITY_INCLUDE_TESTS`.
- `Assets/Animations/`: clips y `Animators/Player.controller`.
- `Assets/Prefabs/`, `Assets/Sprites/`, `Assets/Scenes/SampleScene.unity`.

Los scripts de juego deben quedar dentro de `Scripts` y los tests dentro de `Tests`. Un script fuera de esos assemblies no lo ve el test.

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
   - Detección de suelo mediante `Physics2D.OverlapCircle` (ya implementado) o `BoxCast`, no solo colisiones planas.

## Movimiento y Física (cómo está hoy)
- `Player_controller`: `Update()` lee el input y la detección de suelo; `FixedUpdate()` aplica la velocidad horizontal y el salto.
- El salto se pide en `Update()` con `jumpRequested = true` y se aplica en `FixedUpdate()`, que luego lo limpia.
- Suelo: `Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer)`.
- `Ground_spawner`: crea piezas de `pieceWidth` (20) unidades y borra las que quedan `despawnDistance` (25) detrás del jugador. `pieceWidth` tiene que coincidir con la escala real del prefab `Ground` (no verificado).

## Animación (cómo la manejamos)
No usamos el Animator como máquina de estados por parámetros. El código decide el estado y el Animator solo reproduce el clip pedido.
- `ActualizarAnimaciones()` (en `Update`) elige el estado:
  - En el aire, subiendo: `Player_jumpUp_anim`.
  - En el aire, bajando: `Player_fall_anim`.
  - En el suelo, con velocidad horizontal: `Player_run_anim`.
  - En el suelo, quieto: `Player_idle_anim`.
- `CambiarAnimacion(hash)` guarda el estado actual y llama `animator.Play(hash)` solo si cambió.
- Los hashes salen de `Animator.StringToHash("<nombre>")`. El nombre tiene que ser exactamente el del state en `Player.controller`. Si no coincide, la animación puede fallar en silencio.
- `Player.controller` no tiene transiciones automáticas (`m_Transitions` vacío en todos los states). Si se agregan transiciones, pueden pisar lo que pide el código; revisarlo antes.
- Deslizarse y tropiezo todavía no tienen clip. Cuando existan, el estado se agrega en `ActualizarAnimaciones()` con su propio hash.

## Tests
- Unity Test Runner → PlayMode → `Ground_spawnerTests` (`SpawnsAheadAndRemovesBehind`).
- Un test nuevo va en `Assets/Tests/` y accede a los scripts del juego por el assembly `Scripts`.

## Permisos de Claude (`.claude/settings.json`, `permissions.deny`)
- **Lectura bloqueada:** `Temp/`, `Obj/`, `Build/`, `Builds/`, `UserSettings/`, `MemoryCaptures/`, `*.csproj`, `*.sln`, `*.unityproj`.
- **Escritura bloqueada:** `Temp/`, `Obj/`, `Build/`, `Builds/`, `UserSettings/`, `MemoryCaptures/`. Los archivos generados (`*.csproj`, `*.sln`, `*.unityproj`) no están bloqueados para escritura.
- `Library/` y `Logs/` **no están bloqueados**.
- Escenas, prefabs, assets y `.meta` no están bloqueados: Claude los necesita para resolver referencias por GUID.

## Commits y Pull Requests
- Mensajes de commit en castellano, sin líneas `Co-Authored-By` ni mención a Claude o IA.
- No pushear ni reescribir `main` sin confirmación del equipo.

## Idioma
- Comentarios del código, nombres de textos y documentación en castellano.
