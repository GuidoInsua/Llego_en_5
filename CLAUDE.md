# Contexto del Proyecto: LLEGÓ EN 5' (Runner 2D)

Ver `README.md` para el concepto, los objetivos del proyecto y el recorrido del Nivel 1.

## Estado Actual (prototipo)

Hoy existe en Unity:
- Personaje que corre a velocidad constante y salta (`Player_controller`).
- Suelo generado en tramos delante del jugador y borrado detrás (`Ground_spawner`), que también siembra barriles con probabilidad. Cada tramo (prefab `Ground`, escala 0.4) es la calle `road&lamps` con postes y pared, sin collider en los postes.
- Barriles (`Obstaculo`, prefab `Caja`): toman un color al azar de `variantes` y tienen una luz 2D sutil (`Brillo`). Al chocar (también al caer encima, es a propósito) el jugador baja su velocidad `duracionGolpe` s y el barril titila y desaparece.
- Menú con Play y Exit (`Menu_manager`, escena `Menu`) y fondo parallax (`Parallax_layer`).
- Pausa con Escape en `Level1` (`Menu_pausa`: continuar, menú y salir) y selección inicial con teclado en los menús (`Seleccion_inicial`).
- Música, ambiente y SFX de pasos. `Efectos_sonido` (objeto `Sonidos`) para efectos sueltos y `Volumen_audio` para el volumen guardado; todavía no hay pantalla de opciones que lo cambie. Cámara con Cinemachine y Pixel Perfect Camera.
- El golpe hace titilar al jugador entre su material normal y `DestelloBlanco` (sprite entero blanco).

Todavía no existe: deslizarse, tropiezo, monedas, patada, subte y tienda, reloj, jefe, historieta y arte final.

**Objetivo inmediato (README, corto plazo):** núcleo jugable en un tramo de prueba: correr, saltar, deslizarse y tropiezo.

## Stack y Versión
- **Motor:** Unity 6.6 (6000.6.5f1), 2D Core, Universal Render Pipeline 2D
- **Lenguaje:** C# (.NET / CoreCLR)
- **Input:** New Input System (`UnityEngine.InputSystem`)
- **Física:** Physics2D (`Rigidbody2D`, `Collider2D`)

## Estructura
- `Assets/Scripts/`: código del juego, en subcarpetas por tema: `Player/`, `World/` (suelo, obstáculos, parallax), `UI/` (menús) y `Audio/`. Assembly `Scripts` (`Scripts.asmdef`, referencia `Unity.InputSystem` y `Unity.UI`).
- `Assets/Tests/`: tests PlayMode. Assembly `Tests` (`Tests.asmdef`), referencia `Scripts` y solo compila con `UNITY_INCLUDE_TESTS`.
- `Assets/Animations/`: clips y `Animators/Player.controller`.
- `Assets/Prefabs/` (`Player`, `Ground`, `Caja`), `Assets/Sprites/`, `Assets/Materials/` (`DestelloBlanco`).
- Un script nuevo va en la carpeta de su tema; si no encaja en ninguna, se crea una carpeta nueva en `Scripts/` en vez de dejarlo suelto. Un archivo por clase, con el mismo nombre que la clase.
- No mover ni renombrar archivos desde fuera de Unity sin su `.meta` (mover archivo y `.meta` juntos, o mover desde el Editor). Las escenas guardan los scripts por GUID, pero si se renombra una clase usada en un `OnClick` hay que reconectarlo en el Inspector.
- Escenas: `Assets/Scenes/Menu.unity` (arranque) y `Assets/Scenes/Level1.unity` (nivel, activa al trabajar). Ambas tienen que estar en Build Settings.

Los scripts de juego deben quedar dentro de `Scripts` y los tests dentro de `Tests`. Un script fuera de esos assemblies no lo ve el test.

## Nomenclatura
Lo que ya usa el código, para mantenerlo consistente:

| Qué | Regla | Ejemplo |
|---|---|---|
| Clases y archivos `.cs` | Palabras en castellano unidas con `_`, solo la primera con mayúscula. Una palabra sola, sin `_` | `Player_controller`, `Ground_spawner`, `Menu_pausa`, `Obstaculo` |
| Clases de test | Clase probada + `Tests`; `Test_utils` es la ayuda compartida | `Player_controllerTests` |
| Métodos | PascalCase y verbo. Los mensajes de Unity (`Awake`, `Update`) tal cual | `ActualizarAnimaciones()`, `Romper()` |
| Campos privados y locales | camelCase | `golpeCoroutine`, `alturaSuelo` |
| Campos de Inspector | `[SerializeField] private` + camelCase. Nunca `public` para verlos | `[SerializeField] private float jumpForce` |
| `const` y `static readonly` | PascalCase | `Parpadeo`, `AnimRun`, `ClaveMusica` |
| Booleanos | Que se lean como pregunta: prefijo `is`/`has` | `isGrounded`, `isPausado` |
| Prefabs, escenas, materiales | PascalCase | `Caja`, `Level1`, `DestelloBlanco` |
| Clips y states del Animator | `Player_<estado>_anim`, igual al nombre del state | `Player_jumpUp_anim` |
| Tests | `NombreDeLoQueSeProbaEnPascalCase` | `RunsAtConstantSpeed` |

- **Idioma:** los nombres nuevos van en castellano (clases, campos, métodos). Quedan en inglés los heredados (`runSpeed`, `jumpForce`, `groundCheck`, `Player_controller`) y lo que viene del motor o de una interfaz; no se renombran de golpe: se hace solo con motivo, porque hay referencias en escenas y prefabs.
- Los campos del Inspector no se renombran sin `[FormerlySerializedAs]`: se pierde el valor guardado en la escena.
- Los textos y comentarios van en castellano.

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
5. **Validar referencias al arrancar:** si falta algo obligatorio del Inspector, `Debug.LogError` y `enabled = false`, para no explotar en cada frame. Va en `Awake`; si el script lee un prefab o los tests asignan los campos después de crear el componente (`Ground_spawner`), va en `Start`. Los tests que necesiten un `Awake` válido crean el objeto inactivo, asignan los campos con `Test_utils.SetField` y después lo activan.
6. **Estado de juego:**
   - Daño: cuando haya más de un tipo de cosa que dañe o reciba daño, se hace con una interfaz (`IDamageable`) y no comparando tipos concretos. Hoy solo existe `Obstaculo`.
   - Lo que aparece y desaparece seguido va con pool, reiniciando todo su estado al reusarlo. Hoy los tramos y barriles se instancian y destruyen; el pool se hace cuando el profiler lo pida o con las monedas.
   - Números de balance (daño, velocidad, tiempos, probabilidades) son campos `[SerializeField]`, no valores escritos dentro del método.
   - Sin `Debug.Log` de eventos normales; quedan `LogWarning` y `LogError`. No se agrega código que nadie llama "por si acaso".
7. **Utilidades:** el azar suave (movimiento de enemigos) y los sorteos con pesos (qué obstáculo o moneda sale) se hacen una vez, como clase estática pura que recibe el valor al azar como parámetro (así se puede probar sin Unity). No escribir Perlin ni sorteos a mano en cada script.
8. **Async:** corrutinas para esperas cortas y secuencias lineales; `Awaitable` si hay que devolver un valor o encadenar pasos. Evitar `Task` y `Task.Delay`: asignan memoria y no respetan `Time.timeScale`. El `Awaitable` se espera en el mismo lugar donde se llama, nunca dos veces ni guardado en una variable (viene de un pool). Un método `async` sigue corriendo si el objeto se destruye: pasar `destroyCancellationToken`. Nada de `async void`.

## Sonido y pausa
- Efectos sueltos: `Efectos_sonido.Reproducir(clip)` (objeto `Sonidos` de la escena); evita apilar el mismo clip en pocos milisegundos. No poner un `AudioSource` por efecto. Excepción: un sonido que tiene que cortarse con su objeto, como los pasos (el jugador aplica `Volumen_audio.Efectos` por su cuenta).
- Todo sonido nuevo respeta el volumen elegido en opciones: `Volumen_audio.Efectos` / `Volumen_audio.Musica` (se guarda en `PlayerPrefs`).
- Pausa: `Menu_pausa` pone `Time.timeScale = 0` y `AudioListener.pause = true`, y lo deja en 1 y `false` al continuar, cambiar de escena o destruirse. Todo script que lea input tiene que ignorarlo con `Time.timeScale == 0` (como el salto del jugador). Los tiempos que tienen que avanzar en pausa usan `Time.unscaledTime`.
- Menús: cada menú lleva `Seleccion_inicial` con su primera opción, para manejarlo con flechas y Enter. Los botones llaman a métodos públicos de `Menu_manager` / `Menu_pausa` desde el `OnClick`.

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
- **El juego está diseñado para 16:9.** En otras proporciones la Pixel Perfect Camera está en `Crop Frame` = Windowbox (`m_CropFrame: 3`): barras negras y la vista queda igual que en 16:9. No subir el `orthographicSize` a mano para "adaptar": la Pixel Perfect Camera lo controla. Todo cambio tiene que seguir viéndose igual en 16:9.
- UI: los `Canvas` usan `CanvasScaler` en *Scale With Screen Size*, referencia 1920×1080 y ajuste por alto (`Match` = 1). Una imagen de fondo que llene la pantalla lleva `AspectRatioFitter` en *Envelope Parent* para no deformarse.
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
- El golpe no apaga el sprite: alterna `sharedMaterial` entre el material original y `materialDestello` (`DestelloBlanco`, shader `GUI/Text Shader`) y lo restaura al terminar.
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
