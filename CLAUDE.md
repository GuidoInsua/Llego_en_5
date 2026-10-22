# Contexto del Proyecto: LLEGÓ EN 5' (Runner 2D)

Ver `README.md` para el concepto, los objetivos del proyecto y el recorrido del Nivel 1.

## Estado Actual (prototipo)

Hoy existe en Unity:
- Personaje que corre a velocidad constante y salta (`Player_controller`).
- Suelo generado en tramos delante del jugador y borrado detrás (`Ground_spawner`), que también siembra barriles con probabilidad. Cada tramo (prefab `Ground`, escala 0.4) es la calle `road&lamps` con postes y pared, sin collider en los postes.
- Barriles (`Obstaculo`, prefab `Caja`): toman un color al azar de `variantes` y tienen una luz 2D sutil (`Brillo`). Al chocar (también al caer encima, es a propósito) el jugador baja su velocidad `duracionGolpe` s y el barril titila y desaparece.
- Menú con Play y Exit (`Menu_manager`, escena `Menu`) y fondo parallax (`Parallax_layer`).
- Música, ambiente y SFX de pasos. Cámara con Cinemachine y Pixel Perfect Camera.

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
- `Assets/Prefabs/` (`Player`, `Ground`, `Caja`), `Assets/Sprites/`.
- Escenas: `Assets/Scenes/Menu.unity` (arranque) y `Assets/Scenes/Level1.unity` (nivel, activa al trabajar). Ambas tienen que estar en Build Settings.

Los scripts de juego deben quedar dentro de `Scripts` y los tests dentro de `Tests`. Un script fuera de esos assemblies no lo ve el test.

## Reglas de Arquitectura y Código
1. **Encapsulamiento:** Nunca uses variables `public` para referencias del Inspector; usa `[SerializeField] private`.
2. **Rendimiento:**
   - Prohibido llamar a `GetComponent()`, `Find()`, o instanciar objetos dentro de `Update()` en cada frame. Excepción: `Ground_spawner` instancia un tramo cada ~2,7 s; no se usa pool hasta que el profiler lo pida.
   - Almacena referencias en `Awake()` o `Start()`.
   - Toda interacción con `Rigidbody2D` (fuerzas, velocidad) debe ejecutarse en `FixedUpdate()`.
3. **Unity 6.6 Peculiaridades:**
   - **Fast Enter Play Mode:** En 6.6 el modo por defecto no recarga dominio completo. Si usas eventos estáticos o estados globales, asegúrate de limpiarlos en `OnDisable()` / `OnDestroy()` o usa `[RuntimeInitializeOnLoadMethod]`.
   - No uses wrappers para serializar diccionarios: Unity no serializa `Dictionary<TKey, TValue>` de forma nativa. Usa listas serializables de pares clave/valor, o un `ScriptableObject`.
4. **Mecánicas del Juego (Runner 2D):**
   - El movimiento horizontal es constante o acelerado progresivamente.
   - Detección de suelo mediante `Physics2D.OverlapBox` (ya implementado) o `BoxCast`, no solo colisiones planas.

## Movimiento y Física (cómo está hoy)
- `Player_controller`: `Update()` lee el input y la detección de suelo; `FixedUpdate()` aplica la velocidad horizontal y el salto.
- El salto se pide en `Update()` con `jumpRequested = true` y se aplica en `FixedUpdate()`, que luego lo limpia.
- Suelo: `Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer)`. En la escena la caja es de 0.12 × 0.1 (override sobre el prefab), más angosta que el collider del Player, y el hijo `GroundCheck` queda apenas bajo la suela (y = −0.144). `groundLayer` tiene que ser solo `Ground`; si incluyera la capa del Player, se detectaría a sí mismo.
- `Ground_spawner`: crea tramos y borra los que quedan `despawnDistance` (25) detrás del jugador. En `Start` calcula `pieceWidth` (ancho del sprite hijo `Road` × escala, hoy 7.68) y `alturaSuelo` (borde superior del `BoxCollider2D` del prefab, hoy 0.529); el prefab `Ground` tiene que tener un hijo `Road` y el collider en la raíz.
- Sin "bump" en las uniones: los tramos son hijos de un objeto `Suelo` (creado en `Start`, capa del prefab, `Rigidbody2D` estático + `CompositeCollider2D` en Outlines) y su `BoxCollider2D` está en modo Merge. Outlines es un contorno hueco: `GroundCheck` detecta el suelo porque cruza el borde superior; si quedara entero dentro del suelo, no lo detectaría.

## Cámara y nitidez (pixel art)
- Pixel Perfect Camera de URP en `Main Camera`: PPU 100, referencia 960×540 (2× exacto en 1920×1080), `GridSnapping` en `None` (el snapping hacía vibrar al jugador contra la cámara de Cinemachine). `CinemachineCamera` tiene la extensión `CinemachinePixelPerfect` y ortho 2.7.
- `CinemachineBrain` en `LateUpdate` (el Rigidbody2D del jugador interpola) y damping del `CinemachinePositionComposer` en 0.
- `Parallax_layer` se reposiciona en `RenderPipelineManager.beginCameraRendering`, no en `LateUpdate`: en `LateUpdate` podía correr antes que Cinemachine y temblaba. No volverlo a `LateUpdate`.
- Sprites con filtro Point, sin mipmaps, PPU 100. Las hojas del jugador están cortadas en celdas fijas de 128×128 (con recorte ajustado, el cuerpo saltaba entre frames).
- En el Editor, el VSync del Game view es aparte del de Quality Settings; sin él hay tearing.

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
- **Política: tests a demanda.** En esta etapa no se arman ni se corren tests mientras lo nuevo funcione bien. Se escribe o corre un test solo cuando algo falla o no sale como se quiere. Claude no agrega tests por su cuenta al implementar cambios ni corre la suite de rutina; si el usuario los pide, sí.
- Unity Test Runner → PlayMode, 6 tests, todos pasan: `Ground_spawnerTests` (piezas, cajas y suelo fusionado en un solo collider), `ObstaculoTests` (rotura de la caja) y `Player_controllerTests` (velocidad constante y golpe). No hay tests EditMode.
- Un test nuevo va en `Assets/Tests/` y accede a los scripts del juego por el assembly `Scripts`. `Test_utils.SetField` asigna campos `[SerializeField] private` por reflexión.
- `Destroy` se aplica al final del frame: en un test hay que esperar un frame más antes de comprobar que algo desapareció, y limpiar en `TearDown` con `DestroyImmediate`.
- Los colliders creados por código necesitan `size` explícito, y con `AutoSyncTransforms` apagado hay que llamar `Physics2D.SyncTransforms()` después de mover un objeto.
- Desde la terminal con el Editor abierto: `unity command run_tests --mode PlayMode --async_tests true` y después `unity command test_status --result-only` (sin `--async_tests` devuelve `0/0` y no corre nada). `unity test` por batch solo anda con el Editor cerrado.

## Unity CLI (`unity`, ya en el PATH)
- Con el Editor abierto, `unity status` muestra el puerto del pipeline (`com.unity.pipeline`). Se maneja con `unity command <comando>`; `unity command` sin argumentos lista los disponibles y `--query <texto> --detail full` muestra los parámetros.
- Consola del Editor: `unity command console --result-only`. Estado: `editor_status`. Captura del juego: `capture_game_view` (PNG en base64).
- `eval` ejecuta C# en vivo: la entrada tiene que ser una sentencia terminada en `;` (por ejemplo una llamada). Compilar bloquea el hilo principal; si pasa de 5 s la consola muestra `Main thread operation timed out`, que no es un error del juego.
- Correr `unity ...` desde la raíz del proyecto, sin `cd ... &&` delante: con el prefijo la regla de permisos no coincide y pide confirmación.
- Si el proyecto tiene errores de compilación el Editor entra en Safe Mode y el pipeline no responde (`unity pipeline list` lo confirma).

## Permisos de Claude (`.claude/settings.json`)
- Se quitó el bloque `permissions.deny` a propósito para darle a Claude más acceso con las skills de Unity: ya no hay rutas bloqueadas (`Temp/`, `Obj/`, `Build/`, `UserSettings/`, `*.csproj`, etc.). Igual no hay que editar a mano `Library/`, `Temp/` ni `UserSettings/`.
- `permissions.allow` deja sin confirmación las lecturas del CLI (`unity status`, `doctor`, `logs`, `editors`, `command get_*`, `list_*`, `*_status`, `console`, `capture_*`, `run_tests`). `eval`, `build`, `delete_*` y `package_*` siguen pidiendo confirmación.
- Escenas, prefabs, assets y `.meta` se pueden leer y editar: Claude los necesita para resolver referencias por GUID. Con el Editor abierto, preferir `unity command` a editar YAML a mano.

## Commits y Pull Requests
- Mensajes de commit en castellano, sin líneas `Co-Authored-By` ni mención a Claude o IA.
- No pushear ni reescribir `main` sin confirmación del equipo.

## Idioma
- Comentarios del código, nombres de textos y documentación en castellano.
