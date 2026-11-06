# Contexto del Proyecto: LLEGÓ EN 5' (Runner 2D)

Ver `README.md` para el concepto, los objetivos del proyecto y el recorrido del Nivel 1.

## Estado Actual (prototipo)

Hoy existe en Unity:
- Personaje 3D de perfil (`PlayerSombra`) que corre a velocidad constante, salta y se desliza (`Player_controller`).
- Suelo fijo, armado a mano en la escena (ver **Nivel fijo**): tramos planos separados por un hueco y a distinta altura.
- Obstáculos (`Obstaculo`, prefab `CajaAmarilla`): al chocar (también al caer encima, es a propósito) el jugador baja su velocidad `duracionGolpe` s y el obstáculo titila y desaparece.
- Menú con Play y Exit (`Menu_manager`, escena `Menu`). `Parallax_layer` (fondo parallax) se usa en `Level1` bajo el objeto `Fondos`: `FondoLejano` (`-_Buildings_3`, `seguirCamara` 0,9, orden -20) y `FondoCercano` (`-_Buildings_2`, 0,7, orden -10), cada una con 2 copias, a escala 1,25 (la imagen es 1920×1080 a 100 PPU y la vista mide ~21×12), cada una en su Sorting Layer (`FondoLejano`, `FondoCercano`).
- Pausa con Escape en `Level1` (`Menu_pausa`: continuar, menú y salir) y selección inicial con teclado en los menús (`Seleccion_inicial`).
- Menús placeholder (texto y botones, sin arte propio todavía): `Menu` con Jugar, Opciones (sliders de música y efectos, `Opciones_audio`), Créditos y Salir; la pausa también tiene Opciones. Los paneles se abren con `OnClick` → `GameObject.SetActive` (sin script). Victoria: `Meta_victoria` escucha las `CajaVerde` (`Caja_victoria`, trigger) puestas al final de cada carril; al tocar una se muestra `CanvasVictoria` con el tiempo final (`Cronometro` en `CanvasHUD`, arriba a la derecha; con cada golpe de caja, evento `Player_controller.Golpeado`, hace un pulso de tamaño y titila en rojo 0,6 s) (jugar de nuevo, menú, salir; `Cambio_escena`) y se congela el juego. Derrota: chocar de frente contra la pared de un tramo más alto (`Player_controller.Morir`, contacto con la capa de suelo con normal casi horizontal por delante) deja al jugador quieto en x, titilando y cayendo por el hueco `duracionMuerte` (1,5 s) con los colliders apagados; después se muestra `CanvasGameOver` (reintentar, menú, salir; `Cambio_escena`; copia de `CanvasVictoria`) y se congela el juego. `Player_controller` y `Menu_pausa` tienen el campo `pantallaGameOver`. Estilo de los menús: botones `ButtonSinPulsar2`/`ButtonPulsado2` de 450×100 con texto de 50 (gris 0,196), y el panel de opciones con el tamaño y las posiciones del de `Menu`; el fondo con arte (`Sprites/Menu/MenuBackGround`) solo está en `Menu`, la pausa, victoria y game over siguen con su velo oscuro sobre el juego. La caja verde es solo para probar: se reemplaza por la meta real.
- Música y ambiente (AudioSource de Unity), pasos con FMOD (`Pasos_fmod`) y `Musica_volumen` (aplica `Volumen_audio.Musica` a la música). `Volumen_audio` guarda el volumen; todavía no hay pantalla de opciones que lo cambie. Cámara con Cinemachine.
- El golpe hace titilar al jugador entre su material normal y `DestelloBlanco` (todo el personaje blanco).
- **Estilo sombra 3D:** `Level1` es la escena de juego, con el prefab `PlayerSombra` (modelo `Ch33` de Mixamo, de perfil, en negro y a tamaño real, 1,8 de alto). Sin pixel art: fondo azul liso (color de la cámara), suelo `SueloBloque` (cuadrado gris estirado) y obstáculos `CajaAmarilla` (cuadrado amarillo de 1×1), ambos sobre el sprite `Sprites/Cuadrado` (blanco, 1×1 unidad, pivote abajo al centro). Cámara ortográfica de tamaño 4,5 sin Pixel Perfect y con Cinemachine. La escena de pixel art se borró (queda en el historial de git).

- **Nivel fijo:** el nivel 1 ya no se genera: está armado en `Level1` bajo el objeto `Nivel` (lo hizo una herramienta de una sola vez; se edita a mano). Cada tramo es una instancia del prefab `Tramo` (se cambia el perfil, la capa o el collider ahí para todos; la forma y el índice de `Carril_suelo` son propios de cada instancia). `Suelo0` (abajo), `Suelo1` (medio, donde arranca el jugador) y `Suelo2` (arriba) separados 30 de altura; cada uno tiene un `Tramo_N` por tramo: un `SpriteShapeController` cerrado (perfil `Materials/SueloShapeProfile`, relleno liso negro, sin sprites de borde) con `PolygonCollider2D` (se regenera solo al mover puntos del spline) y `Carril_suelo` con el índice del carril, en la capa `Ground`. Para cambiar el suelo se mueven los puntos del spline (herramienta Edit Spline). `Obstaculos` tiene las cajas (`CajaAmarilla`, flotantes a 1,1 del suelo) y los hidrantes; `Meta_Suelo0/1/2` son paredes de `CajaVerde` (una por carril, en x distintas) que lee `Meta_victoria` (campo `paredes`); `ZonaMuerte` (`Zona_muerte`, trigger) bajo el carril 0 mata al caer por un hueco. Se cambia de carril por eventos: subir (hidrante) y caer por un hueco. Chocar de frente contra una pared de suelo mata solo en el carril 0; en los otros `Player_controller` pasa a `isCayendo`: la cápsula cambia a un `PhysicsMaterial2D` sin fricción, cae por la pared y por el aire de abajo y vuelve al material normal al tocar suelo. Reglas con las que se armó (respetarlas al editar): hueco de 4 antes de un tramo más alto (+0,9 a +1,4) y de 5 antes de uno más bajo (−2 a −3); `profundidad` 12 más lo que se desvían los perfiles más ~5 de salto tiene que ser menor que 30 (separación); cajas a 4 o más de los bordes del tramo y a 8 o más entre sí. Cinemachine tiene el damping vertical en 0.
- **Puentes:** prefab `PuenteSuelo` (Sprite Shape amarillo sólido con `PolygonCollider2D` en la capa `Ground`, `Carril_suelo` 1 y `Puente_suelo`): tapa el hueco entre dos tramos para que no se pueda caer al carril de abajo. Por defecto es un cuadrado de 4 de ancho y 12 de profundidad (igual que el suelo); para hacer rampa se mueven los puntos del spline (arriba: de la punta de un tramo a la del otro). Mientras se lo pisa `Puente_suelo.OnCollisionStay2D` llama a `Player_controller.Ralentizar()` y el jugador va a `multiplicadorVelocidadPuente` (0,7) de su velocidad; se puede subir y bajar sin saltar. Hoy hay 11 (carriles 1 y 2) (`Nivel/Puentes`, `Puente_<carril>_<tramo>_<tramo>`, con las puntas pegadas a los tramos); el resto de los huecos sigue abierto (por ahí se cae al carril 0). Rampas de hasta ~34° (el jugador se pega al suelo y una pared con normal.y < 0,5 mata).
- **Túnel:** prefab `Tunel` (verde, para copiar y pegar), con `Tunel_suelo` y un `BoxCollider2D` trigger en la raíz (la boca; **en la capa Default, no en `Ground`**: el sensor de suelo y el rayo de pegado del jugador lo tomarían por suelo), las piezas de dibujo/colisión armadas a mano con Sprite Shape (`Rampa`, `Rampa (1)` y `Tapa`; las sólidas en `Ground` con `Carril_suelo`) y el hijo vacío `Salida`. Es una acción activable como el hidrante pero hacia abajo y con Shift/S: si el jugador está deslizándose dentro del trigger (`Player_controller.EstaDeslizando`), `Tunel_suelo` apaga el collider de la tapa (campo `tapa`; lo único que se atraviesa) y llama a `Player_controller.EntrarTunel(salida.y + margenSalida)`. De ahí el jugador baja con la **física normal** (choca con las paredes del túnel y con todo el suelo; contra una pared de frente en el carril 1 cae por ella con el material sin fricción, igual que en un hueco), pero queda deslizándose todo el camino (`isEnTunel`: `isDeslizando` fijo, así la animación `Player_slide_anim` no se corta aunque esté en el aire; no responde al salto ni al slide) hasta bajar de la altura de `Salida` + `margenSalida` (2): ahí deja de deslizarse y pasa a fall, land y run; `CarrilCambiado` pone el fondo del carril 0. La tapa se cierra cuando el jugador sale de la boca. Si pasa corriendo, la tapa sólida lo deja seguir por arriba. Dónde cae lo decide la forma de las paredes: tienen que formar un canal que lo lleve al carril de abajo. `Salida` se mueve sobre el suelo del carril 0 que haya debajo (gizmo verde al seleccionar el túnel); hoy hay tres (`Nivel/Tuneles/Tunel_<carril>_<tramo>_<tramo>`: `Tunel_1_1_2`, `Tunel_1_6_7` y `Tunel_2_6_7`). La forma se edita a mano. `Scripts.asmdef` referencia `Unity.2D.SpriteShape.Runtime`.
- **Scripts viejos del generador:** `Ground_spawner`, `Ajustes_suelo`, `Ground_spawnerTests` y el prefab `SueloBloque` se borraron (quedan en el historial de git).
- **Hidrante roto:** prefab `Hidrante` (hijo `Cuerpo` rojo con `Obstaculo`, igual que la caja amarilla, e hijo `Agua` azul con `Chorro_agua`, trigger que crece y se achica en vertical). Va a mano en cualquier carril (en `Nivel/Obstaculos`, `Hidrante_<carril>_<n>`). Es un jump pad: no calcula destinos. El `Chorro_agua` tiene `altura` (slider: cuánto sube sobre el hidrante) y `multiplicadorVelocidad` (1,8: velocidad horizontal del lanzamiento, reemplaza a la de correr); los tramos de arriba se acomodan a mano para que caiga bien (hoy `Hidrante_1_1` 56 y `Hidrante_0_1` 50). Si el jugador toca el agua sin haber chocado con el rojo, `Prompt_tecla` (en `LevelGenerator`, arma un recuadro con la E por código) se muestra `ventana` (0,8 s): con E, `Player_controller.Lanzar(altura, velocidadX)` lo dispara y corre a esa velocidad hasta apoyarse (`isLanzado`). Animación: `Player_jumpUp_anim` todo el vuelo; cada `Tramo` crea en `Awake` (`Zona_tramo`) un trigger hijo sobre su superficie (`Aviso_tramo`, capa Default; en Ground el sensor de suelo lo tomaría por suelo) y al entrar en uno de un carril más alto que el de salida (`Player_controller.EntrarEnTramo`) pasa `retrasoAterrizajeLanzado` (0,5 s) a `Player_jumpLand_anim` y después run. Mientras sube, la cápsula excluye solo la capa Ground (`excludeLayers`; si excluyera todas los triggers no avisarían) y atraviesa el suelo de arriba; en el punto más alto vuelve a chocar y cae sobre él. `Lanzado` lo escucha `Cambio_fondo` (apaga `Fondos` y pasa el cielo a azul oscuro); `Camara_objetivo` sigue al jugador hacia arriba si sube más de `margenArriba` (3).

Todavía no existe: tropiezo, monedas, patada, subte y tienda, reloj, jefe, historieta y arte final.

**Objetivo inmediato (README, corto plazo):** núcleo jugable en un tramo de prueba: correr, saltar, deslizarse y tropiezo.

## Stack y Versión
- **Motor:** Unity 6.6 (6000.6.5f1), 2D Core, Universal Render Pipeline 2D
- **Lenguaje:** C# (.NET / CoreCLR)
- **Input:** New Input System (`UnityEngine.InputSystem`)
- **Física:** Physics2D (`Rigidbody2D`, `Collider2D`)

## Estructura
- `Assets/Scripts/`: código del juego, en subcarpetas por tema: `Player/`, `World/` (suelo, obstáculos, parallax), `UI/` (menús) y `Audio/`. Assembly `Scripts` (`Scripts.asmdef`, referencia `Unity.InputSystem` y `Unity.UI`).
- `Assets/Tests/`: tests PlayMode. Assembly `Tests` (`Tests.asmdef`), referencia `Scripts` y solo compila con `UNITY_INCLUDE_TESTS`.
- `Assets/Animations/Player/`: `PlayerSombra.controller` y `Clips/` (FBX de Mixamo, un clip por archivo: `Player_jumpUp`, `Player_jumpLand`, `Player_fall`, `Player_land`, `Player_slide`). `Assets/Models/Player/Player_modelo_run.fbx`: el modelo con piel y el avatar, y además el clip de correr.
- `Assets/Prefabs/` (`PlayerSombra`, `Tramo`, `CajaAmarilla`, etc.), `Assets/Sprites/`, `Assets/Materials/` (`DestelloBlanco`, `Sombra`), `Assets/StreamingAssets/` (bancos de FMOD, ignorados por git).
- Un script nuevo va en la carpeta de su tema; si no encaja en ninguna, se crea una carpeta nueva en `Scripts/` en vez de dejarlo suelto. Un archivo por clase, con el mismo nombre que la clase.
- No mover ni renombrar archivos desde fuera de Unity sin su `.meta` (mover archivo y `.meta` juntos, o mover desde el Editor). Las escenas guardan los scripts por GUID, pero si se renombra una clase usada en un `OnClick` hay que reconectarlo en el Inspector.
- Escenas: `Assets/Scenes/Menu.unity` (arranque) y `Assets/Scenes/Level1.unity` (nivel de juego). Las dos tienen que estar en Build Settings.

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
| Prefabs, escenas, materiales | PascalCase | `CajaAmarilla`, `Level1`, `DestelloBlanco` |
| Clips y states del Animator | `Player_<estado>_anim`, igual al nombre del state | `Player_jumpUp_anim` |
| Tests | `NombreDeLoQueSeProbaEnPascalCase` | `RunsAtConstantSpeed` |

- **Idioma:** los nombres nuevos van en castellano (clases, campos, métodos). Quedan en inglés los heredados (`runSpeed`, `jumpForce`, `groundCheck`, `Player_controller`) y lo que viene del motor o de una interfaz; no se renombran de golpe: se hace solo con motivo, porque hay referencias en escenas y prefabs.
- Los campos del Inspector no se renombran sin `[FormerlySerializedAs]`: se pierde el valor guardado en la escena.
- Los textos y comentarios van en castellano.

## Reglas de Arquitectura y Código
1. **Encapsulamiento:** Nunca uses variables `public` para referencias del Inspector; usa `[SerializeField] private`.
2. **Rendimiento:**
   - Prohibido llamar a `GetComponent()`, `Find()`, o instanciar objetos dentro de `Update()` en cada frame. No se usa pool hasta que el profiler lo pida.
   - Almacena referencias en `Awake()` o `Start()`.
   - Toda interacción con `Rigidbody2D` (fuerzas, velocidad) debe ejecutarse en `FixedUpdate()`.
3. **Unity 6.6 Peculiaridades:**
   - **Fast Enter Play Mode:** En 6.6 el modo por defecto no recarga dominio completo. Si usas eventos estáticos o estados globales, asegúrate de limpiarlos en `OnDisable()` / `OnDestroy()` o usa `[RuntimeInitializeOnLoadMethod]`.
   - No uses wrappers para serializar diccionarios: Unity no serializa `Dictionary<TKey, TValue>` de forma nativa. Usa listas serializables de pares clave/valor, o un `ScriptableObject`.
4. **Mecánicas del Juego (Runner 2D):**
   - El movimiento horizontal es constante o acelerado progresivamente.
   - Detección de suelo mediante `Physics2D.OverlapBox` (ya implementado) o `BoxCast`, no solo colisiones planas.
5. **Validar referencias al arrancar:** si falta algo obligatorio del Inspector, `Debug.LogError` y `enabled = false`, para no explotar en cada frame. Va en `Awake`; si el script lee un prefab o los tests asignan los campos después de crear el componente, va en `Start`. Los tests que necesiten un `Awake` válido crean el objeto inactivo, asignan los campos con `Test_utils.SetField` y después lo activan.
6. **Estado de juego:**
   - Daño: cuando haya más de un tipo de cosa que dañe o reciba daño, se hace con una interfaz (`IDamageable`) y no comparando tipos concretos. Hoy solo existe `Obstaculo`.
   - Lo que aparece y desaparece seguido va con pool, reiniciando todo su estado al reusarlo. Hoy los tramos y obstáculos se instancian y destruyen; el pool se hace cuando el profiler lo pida o con las monedas.
   - Números de balance (daño, velocidad, tiempos, probabilidades) son campos `[SerializeField]`, no valores escritos dentro del método.
   - Sin `Debug.Log` de eventos normales; quedan `LogWarning` y `LogError`. No se agrega código que nadie llama "por si acaso".
7. **Utilidades:** el azar suave (movimiento de enemigos) y los sorteos con pesos (qué obstáculo o moneda sale) se hacen una vez, como clase estática pura que recibe el valor al azar como parámetro (así se puede probar sin Unity). No escribir Perlin ni sorteos a mano en cada script.
8. **Async:** corrutinas para esperas cortas y secuencias lineales; `Awaitable` si hay que devolver un valor o encadenar pasos. Evitar `Task` y `Task.Delay`: asignan memoria y no respetan `Time.timeScale`. El `Awaitable` se espera en el mismo lugar donde se llama, nunca dos veces ni guardado en una variable (viene de un pool). Un método `async` sigue corriendo si el objeto se destruye: pasar `destroyCancellationToken`. Nada de `async void`.

## Sonido y pausa
- Efectos sueltos: todavía no hay (se borró `Efectos_sonido` por no tener llamadas; está en el historial de git). Cuando haya el primero, sumar un reproductor central y no un `AudioSource` por efecto. Los pasos van por FMOD: Animation Events `Paso` en los clips (en el import del FBX) que llaman a `Pasos_fmod.Paso()`, en el mismo objeto que el Animator (`Modelo`). El evento es `event:/SFX_paso` (campo `EventReference`) y aplica `Volumen_audio.Efectos`; si los bancos no están cargados no suena y no da error. El proyecto de FMOD Studio está en el repo, en `Fmod/pasosEventFmod/` (fuera de `Assets`); ahí se edita y se hace Build (F7), y los bancos de `Fmod/.../Build` van al repo. `sourceBankPath` en `FMODStudioSettings` apunta a esa carpeta y FMOD copia los bancos a `Assets/StreamingAssets/` (ignorado por git) al refrescar (FMOD → Refresh Banks). Las escenas necesitan `AudioListener` y el audio de Unity prendido (Project Settings → Audio → Disable Unity Audio apagado; el instalador de FMOD lo deja prendido) para la música.
- Todo sonido nuevo respeta el volumen elegido en opciones: `Volumen_audio.Efectos` / `Volumen_audio.Musica` (se guarda en `PlayerPrefs`).
- Pausa: `Menu_pausa` pone `Time.timeScale = 0` y `AudioListener.pause = true`, y lo deja en 1 y `false` al continuar, cambiar de escena o destruirse. Todo script que lea input tiene que ignorarlo con `Time.timeScale == 0` (como el salto del jugador). Los tiempos que tienen que avanzar en pausa usan `Time.unscaledTime`.
- Menús: cada menú lleva `Seleccion_inicial` con su primera opción, para manejarlo con flechas y Enter. Los botones llaman a métodos públicos de `Menu_manager` / `Menu_pausa` desde el `OnClick`.

## Movimiento y Física (cómo está hoy)
- `Player_controller`: `Update()` lee el input y la detección de suelo; `FixedUpdate()` aplica la velocidad horizontal y el salto.
- El salto se pide en `Update()` con `jumpRequested = true` y se aplica en `FixedUpdate()`, que luego lo limpia.
- Suelo: `Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer)`. En la escena la caja es de 0.12 × 0.1 (override sobre el prefab), más angosta que el collider del Player, y el hijo `GroundCheck` queda apenas bajo la suela (y = −0.144). `groundLayer` tiene que ser solo `Ground`; si incluyera la capa del Player, se detectaría a sí mismo.
- Deslizarse: Shift (izquierdo o derecho), flecha abajo o S estando en el suelo. Si se aprieta hasta `bufferDeslizar` (0,15 s) antes de tocar el suelo, el pedido se guarda y desliza al aterrizar. `Update()` pide con `slideRequested`; `FixedUpdate()` baja la `CapsuleCollider2D` a `alturaDeslizando` (0,8) con los pies en el mismo lugar durante `duracionDeslizamiento` (1,2 s) y después la devuelve. Termina antes si deja de tocar el suelo. Se puede saltar a los `tiempoMinimoParaSaltar` (0,05 s, ~3 frames) de empezar a deslizarse: el salto corta el deslizamiento. Solo anda si hay `CapsuleCollider2D` y el Animator tiene el state `Player_slide_anim` (`puedeDeslizar`). Hoy no hay techos que lo bloqueen al pararse.
- Pendientes: `Player_controller` se pega al suelo (`distanciaPegado` 0,4, no si cae más rápido que `velocidadMaximaPegado` 8, ni durante un salto): en `FixedUpdate` un raycast hacia abajo da la normal y la velocidad vertical sigue la pendiente con la x constante. Sin eso, al bajar una rampa flotaba ~0,4 s. Rampas de hasta ~34° (ver `rangoLargoPorAltura`); más empinadas harían que una pared (normal.y < 0,5) no se pegue.

## Capas de dibujo (Sorting Layers)
Para saber qué va delante de qué se usan Sorting Layers con nombre y no números de `Order in Layer` (en 0). De atrás hacia adelante, como están en Project Settings → Tags and Layers: `FondoLejano`, `FondoCercano`, `Default`, `Suelo`, `Obstaculos`. Un objeto nuevo se asigna a su capa en el Inspector (en el prefab si es uno). El suelo, las cajas y el resto toman la capa de su prefab. El jugador es un modelo 3D (no es sprite) y no usa estas capas. Todo lo que se dibuja tiene que ser `SpriteRenderer` con el mismo material de sprite (cola 3000): un `MeshRenderer` con ese material ignoraba la Sorting Layer y quedaba siempre detrás de los sprites; por eso el suelo ya no es un mesh.

## Cámara
- Sin Pixel Perfect Camera: la cámara es ortográfica de tamaño 4,5. Si vuelve el pixel art (PPU 100, referencia 960×540, `GridSnapping` en `None`, `CinemachinePixelPerfect`), la configuración anterior está en el historial de git.
- La cámara no sigue al jugador directo sino a `CamaraObjetivo` (`Camara_objetivo`): x del jugador, y a la altura del último suelo (no rebota con los saltos), baja pegado al jugador si cae más de `margenAbajo` (1) y se desliza al suelo nuevo al aterrizar (`tiempoSuavizado` 0,3). La zona muerta del composer está apagada y el damping en 0. Con la zona muerta vertical, en las caídas de 5–8 el jugador llegaba a salirse por abajo de la pantalla.
- `CinemachineBrain` en `LateUpdate` (el Rigidbody2D del jugador interpola) y damping del `CinemachinePositionComposer` en 0.
- `Parallax_layer` se reposiciona en `RenderPipelineManager.beginCameraRendering`, no en `LateUpdate`: en `LateUpdate` podía correr antes que Cinemachine y temblaba. No volverlo a `LateUpdate`.
- Sprites con filtro Point, sin mipmaps, PPU 100 (convención para el pixel art futuro).
- **El juego está diseñado para 16:9.** No subir el `orthographicSize` a mano para "adaptar" a otras proporciones; todo cambio tiene que seguir viéndose igual en 16:9.
- UI: los `Canvas` usan `CanvasScaler` en *Scale With Screen Size*, referencia 1920×1080 y ajuste por alto (`Match` = 1). Una imagen de fondo que llene la pantalla lleva `AspectRatioFitter` en *Envelope Parent* para no deformarse.
- En el Editor, el VSync del Game view es aparte del de Quality Settings; sin él hay tearing.

## Animación (cómo la manejamos)
No usamos el Animator como máquina de estados por parámetros. El código decide el estado y el Animator solo reproduce el clip pedido.
- `ActualizarAnimaciones()` (en `Update`) elige el estado:
  - En el aire, subiendo: `Player_jumpUp_anim`.
  - En el aire, bajando: `Player_fall_anim`.
  - En el suelo, con velocidad horizontal: `Player_run_anim`.
  - En el suelo, quieto: `Player_idle_anim`.
  - Deslizándose (tiene prioridad sobre los demás): `Player_slide_anim`.
- `CambiarAnimacion(hash)` guarda el estado actual y cambia solo si cambió: con `duracionTransicion` > 0 mezcla con `CrossFadeInFixedTime` (0,12 s en `PlayerSombra`: sin transiciones definidas en el Animator igual se mezclan los clips); con 0 corta con `Play`.
- **Aterrizaje con sensor:** `Update()` hace un `BoxCast` hacia abajo desde `groundCheck`, solo contra `groundLayer` (un obstáculo está en otra capa y no lo activa), con la distancia que se cae en `anticipoAterrizaje` (0,12 s; con velocidad y gravedad) o `distanciaMinimaAterrizaje`. Si lo ve (`isPorAterrizar`) y el estado era `jumpUp` pasa a `Player_jumpLand_anim`; si era `fall`, a `Player_land_anim`. El clip se deja terminar (`finAterrizaje`) aunque toque el suelo en el medio, y recién después vuelve a correr; saltar o deslizarse lo corta. Si toca el suelo sin que el sensor lo vea, el aterrizaje arranca igual al tocarlo. Solo si el Animator tiene los dos states.
- Los hashes salen de `Animator.StringToHash("<nombre>")`. El nombre tiene que ser exactamente el del state en `PlayerSombra.controller`. Si no coincide, la animación puede fallar en silencio.
- `PlayerSombra.controller` no tiene transiciones automáticas (`m_Transitions` vacío en todos los states). Si se agregan transiciones, pueden pisar lo que pide el código; revisarlo antes.
- El golpe no apaga el modelo: alterna `sharedMaterial` entre el material original y `materialDestello` (`DestelloBlanco`, shader `GUI/Text Shader`) y lo restaura al terminar.
- **`PlayerSombra`** (modelo 3D de Mixamo): 
- Clips de Mixamo: están en `Assets/Animations/Player/Clips/` (más el de correr, que es el modelo `Assets/Models/Player/Player_modelo_run.fbx`) (Humanoid; el avatar y el modelo salen de `Player_modelo_run`, los otros FBX copian el avatar): `Player_run_anim` y `Player_idle_anim` (provisorio) = `Player_modelo_run`, `Player_jumpUp_anim` = `Player_jumpUp` (sin loop: se queda en el último cuadro), `Player_jumpLand_anim` = `Player_jumpLand`, `Player_fall_anim` = `Player_fall` (en loop), `Player_land_anim` = `Player_land`, `Player_slide_anim` = `Player_slide`. Bajar de Mixamo "In Place" cuando se pueda, FBX Binary, con el mismo personaje.
- (Histórico) Con un solo clip de salto, pasar de `jumpUp` a `fall` en el punto más alto reiniciaba el clip desde la agachada (`Animator.Play` empieza de 0). Por eso `Player_controller` tiene `mantenerSaltoEnDescenso` (activo en `PlayerSombra`): en el aire se queda en `Player_jumpUp_anim` de punta a punta (el clip no es loop: si dura más que el salto, queda en el último cuadro) y pasa a `Player_fall_anim` solo si se cayó sin saltar (`isSaltoActivo` falso) o cae más rápido que `velocidadParaCaer` (12; un salto normal llega a ~11 al aterrizar).
- `Falling To Landing` y `Jump To Landing_Cut` se importaron con *Bake Into Pose* en Position Y (*Original*): sin eso el cuerpo quedaba en el aire durante el aterrizaje porque se descartaba la altura de la cadera (medido con la altura de los pies sobre la cápsula). `Jump To Landing_Cut` arranca a `inicioAterrizajeSalto` (0,2 s): su primera parte es la bajada en el aire, que ya la da la física; si no, los pies empiezan ~1 por encima. `JumpUp_Cut` y los demás clips de aire sí descartan la altura.
- `Running Slide` no ofrece "In Place" en Mixamo: trae el desplazamiento hacia adelante (~6 m) y un giro del cuerpo. Se arregló en el import del FBX (clip → Root Transform): Rotation y Position Y con *Bake Into Pose* + *Original*, Position XZ sin bake. Así el avance se descarta (lo da la física) y el cuerpo conserva la inclinación. Un clip nuevo que no sea in place se configura igual; si tiene el giro del cuerpo, se ve torcido en la vista de perfil.
- Tropiezo todavía no tiene clip. Cuando exista, el estado se agrega en `ActualizarAnimaciones()` con su propio hash.

## Tests
- **Política: tests a demanda.** En esta etapa no se arman ni se corren tests mientras lo nuevo funcione bien. Se escribe o corre un test solo cuando algo falla o no sale como se quiere. Claude no agrega tests por su cuenta al implementar cambios ni corre la suite de rutina; si el usuario los pide, sí.
- Unity Test Runner → PlayMode, `Sorteo_ponderadoTests`, `ObstaculoTests` (rotura de la caja) y `Player_controllerTests` (velocidad constante y golpe). No hay tests EditMode.
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

## Cabecera de los scripts

Todo script `.cs` nuevo (también los de prueba) empieza con esta cabecera, seguida de una línea en blanco y luego los `using`. La última línea, `Desc:`, es lo único que cambia: una frase corta sobre lo que hace ese script. El resto va tal cual. Está copiada de `Assets/Scripts/UI/Cronometro.cs`; si se cambia el diseño, cambiarlo en todos los scripts. Sin mails.

```
// ###########################
// ###############      ######
// ##############       ######
// ###############     #######
// ########         ##########   LLEGÓ EN 5'
// ######    #        ###   ##   -----------
// #####   ###         #   ###   Equipo:  Lucas, Rodrigo, Fatima y Guido
// ##########             ####   Repo:    Llego_en_5
// ##########      ###########   Materia: TP 2 Desarrollo Consciente 2do Cuatri 2026
// #  #####         ##########
// #                  ########
// ####      #####     #######
// ################    #######
// #################   #######
// ##################  #######
// ###########################
// Desc: Tiempo de la partida en el HUD, con efecto al recibir un golpe.
```

## Commits y Pull Requests
- **Sin Git LFS:** la cuenta de GitHub tiene el presupuesto de LFS en 0 (no se puede subir ni bajar nada por LFS). Desde nov. 2026 los binarios van en git normal (`.gitattributes` los marca `binary`); los commits viejos siguen con punteros LFS. No volver a poner `lfs` en `.gitattributes`. GitHub rechaza archivos de más de 100 MB. FMOD solo tiene las librerías de Windows (`platforms/win/lib`); para compilar a otra plataforma hay que reinstalar el plugin.
- Mensajes de commit en castellano, sin líneas `Co-Authored-By` ni mención a Claude o IA.
- No pushear ni reescribir `main` sin confirmación del equipo.

## Idioma
- Comentarios del código, nombres de textos y documentación en castellano.
