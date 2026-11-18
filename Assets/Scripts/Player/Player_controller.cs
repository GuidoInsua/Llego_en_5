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
// Desc: Controla al jugador: correr, saltar, deslizarse, golpes, muerte y animaciones.

using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

// El Animator y los renderers pueden estar en un hijo (el modelo 3D)
[RequireComponent(typeof(Rigidbody2D))]
public class Player_controller : MonoBehaviour
{
	[Header("Movimiento")]
	[SerializeField] private float runSpeed = 7f;
	[Tooltip("Velocidad a la que el clip de correr se ve bien a velocidad normal: con más o menos velocidad el clip se acelera o frena en proporción.")]
	[SerializeField] private float velocidadClipCorrer = 7f;
	[SerializeField] private float jumpForce = 13f;
	[Tooltip("Unidades/s² con las que vuelve a la velocidad normal tras un golpe o un puente. Frenar es instantáneo; el lanzamiento del hidrante también.")]
	[SerializeField] private float aceleracion = 15f;

	[Header("Salto")]
	[Tooltip("Si se aprieta saltar un instante antes de tocar el suelo, el pedido se guarda este tiempo y salta al aterrizar.")]
	[SerializeField] private float bufferSalto = 0.12f;
	[Tooltip("Coyote time: segundos después de dejar el suelo (por un borde) en que todavía se puede saltar.")]
	[SerializeField] private float tiempoCoyote = 0.08f;
	[Tooltip("Tres saltos según cuánto se mantiene la tecla: soltarla enseguida da el corto, un poco después el medio, y mantenerla da el largo (jumpForce). Altura de los pies sobre el despegue.")]
	[SerializeField] private float alturaSaltoCorto = 1.1f;
	[SerializeField] private float alturaSaltoMedio = 1.7f;

	[Header("Aire")]
	[SerializeField] private float velocidadParaCaer = 12f; // Velocidad de caída por encima de la de un salto normal (~11): pasa a Fall

	[Header("Transiciones")]
	[Tooltip("Segundos de mezcla entre un clip y el siguiente. 0 = corte directo.")]
	[SerializeField] private float duracionTransicion = 0f;

	[Header("Aterrizaje")]
	[Tooltip("Segundos antes de tocar el suelo en que arranca el clip de aterrizaje. El sensor mira hacia abajo velocidadDeCaída × este tiempo.")]
	[SerializeField] private float anticipoAterrizaje = 0.12f;
	[SerializeField] private float distanciaMinimaAterrizaje = 0.3f; // El sensor nunca mira menos que esto
	[Tooltip("Segundo del clip Jump To Landing en que arranca. La primera parte del clip es la bajada en el aire, que ya la da la física: arrancando a 0,2 s los pies empiezan cerca del suelo y el contacto cae a ~0,1 s.")]
	[SerializeField] private float inicioAterrizajeSalto = 0.2f;
	[Tooltip("Cuánto dura desde ese punto: el largo del clip Jump To Landing (0,6 s) menos el inicio.")]
	[SerializeField] private float duracionAterrizajeSalto = 0.4f;
	[Tooltip("Duración del clip Falling To Landing, tras una caída larga.")]
	[SerializeField] private float duracionAterrizajeCaida = 0.5f;

	[Header("Deslizarse")]
	[SerializeField] private float duracionDeslizamiento = 1.2f;
	[Tooltip("Si se aprieta deslizar un instante antes de tocar el suelo, el pedido se guarda este tiempo y se desliza al aterrizar.")]
	[SerializeField] private float bufferDeslizar = 0.15f;
	[Tooltip("Cuánto después de empezar a deslizarse ya se puede saltar (unos 3 frames): corta el deslizamiento sin esperar a que termine, pero no permite deslizar y saltar en el mismo instante.")]
	[SerializeField] private float tiempoMinimoParaSaltar = 0.05f;
	[SerializeField] private float alturaDeslizando = 0.8f; // Alto de la cápsula mientras se desliza: los pies quedan en el mismo lugar

	[Header("Golpe")]
	[SerializeField] private float duracionGolpe = 1.5f;
	[SerializeField] private float multiplicadorVelocidadGolpe = 0.5f;

	[Header("Puente")]
	[Tooltip("Velocidad (proporción de la normal) mientras pisa un Puente_suelo.")]
	[SerializeField] private float multiplicadorVelocidadPuente = 0.7f;

	[Header("Muerte")]
	[Tooltip("Chocar de frente contra el suelo (la pared de un tramo más alto): titila y cae por el hueco este tiempo, y después aparece el game over.")]
	[SerializeField] private float duracionMuerte = 0.6f;
	[Tooltip("Empieza apagada; se prende al terminar de caer.")]
	[SerializeField] private GameObject pantallaGameOver;

	[SerializeField] private Material materialDestello; // Pinta el personaje entero de blanco (shader GUI/Text Shader)

	[Header("Polvo")]
	[Tooltip("Partículas en los pies (opcional): una ráfaga al aterrizar y al empezar a deslizarse.")]
	[SerializeField] private ParticleSystem polvo;
	[SerializeField] private int polvoAterrizaje = 10;
	[SerializeField] private int polvoDeslizamiento = 8;
	[Tooltip("Caídas más lentas que esto aterrizan sin polvo (un salto normal llega a ~11).")]
	[SerializeField] private float velocidadMinimaPolvo = 9f;

	[Header("Sonidos (FMOD, opcionales)")]
	[SerializeField] private EventReference sonidoSalto;
	[SerializeField] private EventReference sonidoSuperSalto; // El lanzamiento del hidrante
	[SerializeField] private EventReference sonidoCaida; // Al aterrizar
	[SerializeField] private EventReference sonidoBarrida;
	[SerializeField] private EventReference sonidoRecolectar; // Al juntar una excusa
	[Tooltip("Caídas más lentas que esto aterrizan sin sonido: los saltitos de las rampas no suenan (el salto corto llega a ~8).")]
	[SerializeField] private float velocidadMinimaSonidoCaida = 4f;

	[Header("Detección de Suelo")]
	[SerializeField] private Transform groundCheck;
	[SerializeField] private Vector2 groundCheckSize = new Vector2(0.15f, 0.05f); // Más angosta que los pies para no detectar paredes
	[SerializeField] private LayerMask groundLayer;
	[Tooltip("Hasta qué distancia bajo los pies el jugador se pega al suelo en las pendientes. Más que eso se cae.")]
	[SerializeField] private float distanciaPegado = 0.4f;
	[Tooltip("Si cae más rápido que esto no se pega: las caídas grandes no se frenan antes de tocar el suelo.")]
	[SerializeField] private float velocidadMaximaPegado = 8f;

	// Nombres de los states del Animator: tienen que ser iguales a los de PlayerSombra.controller
	private static readonly int AnimIdle = Animator.StringToHash("Player_idle_anim");
	private static readonly int AnimRun = Animator.StringToHash("Player_run_anim");
	private static readonly int AnimJumpUp = Animator.StringToHash("Player_jumpUp_anim");
	private static readonly int AnimFall = Animator.StringToHash("Player_fall_anim");
	private static readonly int AnimSlide = Animator.StringToHash("Player_slide_anim");
	private static readonly int AnimLand = Animator.StringToHash("Player_land_anim");
	private static readonly int AnimJumpLand = Animator.StringToHash("Player_jumpLand_anim");

	// Espera entre titiladas. Se crea una sola vez y se reutiliza, para no crear una nueva cada vez
	private static readonly WaitForSeconds Parpadeo = new WaitForSeconds(0.1f);
	// La lentitud del puente dura un poco más que el último paso sobre él, para que no titile
	private const float DuracionLentitud = 0.1f;
	// El rayo que busca la pendiente arranca un poco arriba de los pies
	private const float AlturaRayo = 0.1f;

	// Avisos para otros scripts
	public event System.Action Golpeado; // Al chocar un obstáculo. Lo escuchan Cronometro y Sacudida_camara
	public event System.Action Ralentizado; // Al empezar a pisar un puente (no en cada paso). Lo escucha Cronometro
	public event System.Action<Transform> Recolectado; // Con la excusa tocada. Lo escucha Contador_recolectables
	public event System.Action<float> Aterrizado; // Al tocar el suelo, con la velocidad de caída más alta. Lo escucha Sacudida_camara
	public event System.Action<Carril> CarrilCambiado; // Al pisar el suelo de otro carril. Lo escucha Cambio_fondo

	// Componentes
	private Rigidbody2D rb;
	private Animator animator;
	private Renderer[] renderizadores;
	private Material[] materialesNormales; // Uno por renderer, en el mismo orden
	private CapsuleCollider2D capsula;
	private Vector2 capsulaSizeNormal;
	private Vector2 capsulaOffsetNormal;
	private PhysicsMaterial2D materialNormal;
	private PhysicsMaterial2D materialSinFriccion; // Sin esto, empujando contra la pared se queda pegado en vez de caer
	private LayerMask exclusionNormal;
	private bool puedeAterrizar; // Solo si el Animator tiene los states de aterrizaje
	private bool puedeDeslizar; // Solo si hay cápsula y el Animator tiene el clip

	// Suelo
	private bool isGrounded;
	private Collider2D ultimoSuelo; // Para buscar Carril_suelo solo cuando cambia el suelo
	private bool hasCarril; // Falso hasta pisar el primer suelo
	private Carril carrilActual;
	private bool isPorAterrizar; // El sensor ve el suelo (no un obstáculo) justo debajo de los pies
	private float velocidadCaida; // La más rápida de esta caída, para el polvo, el sonido y la cámara al aterrizar

	// Velocidad horizontal
	private float velocidadActual; // Frena de golpe y acelera de a poco
	private float lentoHasta; // Hasta qué momento va lento por el puente
	private Coroutine golpeCoroutine; // No es null mientras dura el golpe

	// Salto
	private bool jumpRequested; // Update lo pide y FixedUpdate lo aplica
	private float fuerzaPedida; // Velocidad hacia arriba del próximo salto: jumpForce, o la del lanzamiento
	private bool isSaltoActivo; // Desde que despega con un salto hasta que vuelve a tocar el suelo
	private bool isCorteSaltoPedido; // Soltó la tecla mientras subía: FixedUpdate baja la velocidad
	private float alturaDespegue; // Altura del jugador al saltar: el corte mide desde acá
	// Cuándo se pidió el último salto y cuándo estuvo en el suelo por última vez (para el buffer y el coyote time).
	// Arrancan en "hace muchísimo tiempo": al empezar no hay ningún pedido guardado
	private float ultimoPedidoSalto = float.NegativeInfinity;
	private float ultimoEnSuelo = float.NegativeInfinity;

	// Lanzamiento del hidrante
	private bool isLanzamientoPedido; // Lanzar() lo pide y FixedUpdate lo aplica
	private bool isLanzado; // Desde el lanzamiento hasta volver a apoyarse: corre a velocidadLanzado
	private float velocidadLanzado;
	private float inicioLanzado;
	private bool isAtravesando; // Sube atravesando el suelo hasta el punto más alto

	// Caída por un hueco de la calle o los edificios
	private bool isCayendoPedido; // OnCollisionEnter2D lo pide y FixedUpdate lo aplica
	private bool isCayendo; // Cae pegado a la pared sin fricción hasta volver a tocar el suelo
	private float inicioCaida;

	// Deslizarse
	private bool slideRequested;
	private bool isDeslizando;
	private float inicioDeslizamiento;
	private float finDeslizamiento;
	private float ultimoPedidoDeslizar = float.NegativeInfinity;

	// Animación
	private int animacionActual;
	private float finAterrizaje; // Hasta cuándo se deja terminar el clip de aterrizaje

	private bool isMuerto;

	// Lo usan el hidrante y la alcantarilla para no hacer nada si el jugador ya murió
	public bool EstaMuerto
	{
		get { return isMuerto; }
	}

	// Lo usa el hidrante para calcular la velocidad horizontal del lanzamiento
	public float VelocidadCarrera
	{
		get { return runSpeed; }
	}

	// La cámara lo usa para saber cuándo el jugador apoyó los pies
	public bool EstaEnSuelo
	{
		get { return isGrounded; }
	}

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponentInChildren<Animator>();

		// Solo los renderers del modelo (los que están bajo el Animator): las partículas del polvo no titilan
		if (animator != null)
		{
			renderizadores = animator.GetComponentsInChildren<Renderer>();
		}
		else
		{
			renderizadores = new Renderer[0];
		}

		if (groundCheck == null || materialDestello == null || pantallaGameOver == null || animator == null || renderizadores.Length == 0)
		{
			Debug.LogError("Player_controller: falta asignar groundCheck, materialDestello o pantallaGameOver, o no hay Animator ni Renderer en el jugador o sus hijos.");
			enabled = false;
			return;
		}

		// Guarda el material de cada parte del modelo, para devolverlo después del destello blanco
		materialesNormales = new Material[renderizadores.Length];
		for (int i = 0; i < renderizadores.Length; i++)
		{
			materialesNormales[i] = renderizadores[i].sharedMaterial;
		}

		velocidadActual = runSpeed;
		capsula = GetComponent<CapsuleCollider2D>();
		puedeAterrizar = animator.HasState(0, AnimLand) && animator.HasState(0, AnimJumpLand);
		puedeDeslizar = capsula != null && animator.HasState(0, AnimSlide);

		if (capsula != null)
		{
			capsulaSizeNormal = capsula.size;
			capsulaOffsetNormal = capsula.offset;
			exclusionNormal = capsula.excludeLayers;
			materialNormal = capsula.sharedMaterial;

			materialSinFriccion = new PhysicsMaterial2D("SinFriccion");
			materialSinFriccion.friction = 0f;
			materialSinFriccion.bounciness = 0f;
		}
	}

	// ---------- Update: input, suelo y animaciones ----------

	void Update()
	{
		DetectarSuelo();
		RevisarSensorAterrizaje();
		LeerSalto();
		LeerDeslizamiento();
		ActualizarAnimaciones();
		AjustarVelocidadDelClip();
	}

	// Se fija si está pisando el suelo. Al aterrizar suelta el polvo, el sonido y avisa
	private void DetectarSuelo()
	{
		Collider2D suelo = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
		bool estabaEnSuelo = isGrounded;

		// Mientras atraviesa el suelo en el lanzamiento no cuenta como apoyado (OverlapBox no respeta el excludeLayers de la cápsula)
		isGrounded = suelo != null && !isAtravesando;

		if (isGrounded)
		{
			ultimoEnSuelo = Time.time;

			if (suelo != ultimoSuelo)
			{
				ultimoSuelo = suelo;
				RevisarCambioDeCarril(suelo);
			}

			if (!estabaEnSuelo && !isMuerto)
			{
				EfectosDeAterrizaje();
			}
			velocidadCaida = 0f;
		}
		else
		{
			// Guarda la velocidad de caída más alta (al caer la velocidad y es negativa)
			velocidadCaida = Mathf.Max(velocidadCaida, -rb.linearVelocity.y);
		}
	}

	// Si el suelo nuevo es de otro carril, avisa (Cambio_fondo cambia el fondo)
	private void RevisarCambioDeCarril(Collider2D suelo)
	{
		Carril_suelo carrilSuelo = suelo.GetComponent<Carril_suelo>();
		if (carrilSuelo == null)
		{
			return;
		}

		if (!hasCarril || carrilSuelo.Carril != carrilActual)
		{
			hasCarril = true;
			carrilActual = carrilSuelo.Carril;
			if (CarrilCambiado != null)
			{
				CarrilCambiado(carrilActual);
			}
		}
	}

	private void EfectosDeAterrizaje()
	{
		if (velocidadCaida >= velocidadMinimaPolvo)
		{
			SoltarPolvo(polvoAterrizaje);
		}
		if (velocidadCaida >= velocidadMinimaSonidoCaida)
		{
			Sonido_fmod.Tocar(sonidoCaida);
		}
		if (Aterrizado != null)
		{
			Aterrizado(velocidadCaida);
		}
	}

	// Sensor de aterrizaje: mientras cae, mira si el suelo está justo debajo de los pies.
	// Solo mira la capa Ground, así un obstáculo en el camino no lo activa
	private void RevisarSensorAterrizaje()
	{
		isPorAterrizar = false;

		bool isBajando = !isGrounded && rb.linearVelocity.y < -0.5f;
		if (!isBajando)
		{
			return;
		}

		// Cuánto va a caer en los próximos `anticipoAterrizaje` segundos: distancia = v·t + ½·g·t²
		float tiempo = anticipoAterrizaje;
		float velocidadBajada = -rb.linearVelocity.y;
		float distanciaPorVelocidad = velocidadBajada * tiempo;
		float distanciaPorGravedad = 0.5f * CalcularGravedad() * tiempo * tiempo;
		float distancia = distanciaPorVelocidad + distanciaPorGravedad;

		// Como mínimo mira distanciaMinimaAterrizaje
		distancia = Mathf.Max(distanciaMinimaAterrizaje, distancia);

		RaycastHit2D golpe = Physics2D.BoxCast(groundCheck.position, groundCheckSize, 0f, Vector2.down, distancia, groundLayer);
		isPorAterrizar = golpe.collider != null;
	}

	// Salto con W o flecha arriba (el Espacio es interactuar). En pausa (timeScale 0) se ignora
	private void LeerSalto()
	{
		bool jumpPressed = false;
		bool jumpReleased = false;
		Keyboard teclado = Keyboard.current;
		if (Time.timeScale > 0f && teclado != null)
		{
			jumpPressed = teclado.wKey.wasPressedThisFrame || teclado.upArrowKey.wasPressedThisFrame;
			jumpReleased = teclado.wKey.wasReleasedThisFrame || teclado.upArrowKey.wasReleasedThisFrame;
		}

		// El pedido se guarda un ratito (buffer): si se apretó justo antes de tocar el suelo, salta al aterrizar
		if (jumpPressed)
		{
			ultimoPedidoSalto = Time.time;
		}

		// Puede saltar si está en el suelo, o si dejó un borde hace muy poco sin saltar (coyote time)
		bool isCoyote = !isSaltoActivo && !isAtravesando && Time.time - ultimoEnSuelo <= tiempoCoyote;
		bool isApoyado = isGrounded || isCoyote;
		bool isPedidoVigente = Time.time - ultimoPedidoSalto <= bufferSalto;
		// Recién empezó a deslizarse: todavía no puede saltar
		bool isDeslizandoHacePoco = isDeslizando && Time.time < inicioDeslizamiento + tiempoMinimoParaSaltar;

		if (isApoyado && isPedidoVigente && !isDeslizandoHacePoco)
		{
			// Si ya hay un lanzamiento del hidrante pedido, no se pisa su fuerza
			if (!jumpRequested)
			{
				fuerzaPedida = jumpForce;
			}
			jumpRequested = true;
			ultimoPedidoSalto = float.NegativeInfinity;
		}

		// Soltar la tecla mientras sube corta el salto. Si se soltó antes de despegar (salto guardado en el buffer), el salto es completo
		if (jumpReleased && isSaltoActivo && !isLanzado)
		{
			isCorteSaltoPedido = true;
		}

		// En el suelo y sin subir: el salto terminó
		if (isGrounded && rb.linearVelocity.y <= 0.1f)
		{
			isSaltoActivo = false;
		}
	}

	// Deslizarse (barrida) con S o flecha abajo, solo desde el suelo.
	// Si se aprieta justo antes de aterrizar, el pedido se guarda `bufferDeslizar` segundos
	private void LeerDeslizamiento()
	{
		bool slidePressed = false;
		Keyboard teclado = Keyboard.current;
		if (Time.timeScale > 0f && teclado != null)
		{
			slidePressed = teclado.downArrowKey.wasPressedThisFrame || teclado.sKey.wasPressedThisFrame;
		}

		if (slidePressed)
		{
			ultimoPedidoDeslizar = Time.time;
		}

		bool isPedidoVigente = Time.time - ultimoPedidoDeslizar <= bufferDeslizar;
		if (puedeDeslizar && isGrounded && !isDeslizando && isPedidoVigente)
		{
			slideRequested = true;
			ultimoPedidoDeslizar = float.NegativeInfinity;
		}
	}

	// El clip de correr va más rápido o más lento según la velocidad; los demás, a velocidad normal
	private void AjustarVelocidadDelClip()
	{
		if (animacionActual == AnimRun && velocidadClipCorrer > 0f)
		{
			animator.speed = Mathf.Abs(rb.linearVelocity.x) / velocidadClipCorrer;
		}
		else
		{
			animator.speed = 1f;
		}
	}

	// ---------- FixedUpdate: física ----------

	void FixedUpdate()
	{
		// Muerto: queda quieto en x y cae por la gravedad
		if (isMuerto)
		{
			rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
			return;
		}

		MoverHaciaAdelante();
		AplicarSalto();
		ActualizarCaidaPorHueco();
		ActualizarLanzamiento();
		ActualizarDeslizamiento();
	}

	// Velocidad horizontal y, en las rampas, la vertical que sigue al suelo
	private void MoverHaciaAdelante()
	{
		// Frenar (golpe, puente) y el lanzamiento son instantáneos; volver a la velocidad normal es de a poco
		float objetivo = CalcularVelocidadObjetivo();
		if (isLanzado || objetivo < velocidadActual)
		{
			velocidadActual = objetivo;
		}
		else
		{
			velocidadActual = Mathf.MoveTowards(velocidadActual, objetivo, aceleracion * Time.fixedDeltaTime);
		}

		float velocidadX = velocidadActual;
		float velocidadY = rb.linearVelocity.y;

		// En una pendiente la velocidad sigue al suelo: sin esto, al bajar una rampa se despega y flota
		bool puedePegarse = !jumpRequested && !isSaltoActivo && velocidadY > -velocidadMaximaPegado;
		if (puedePegarse)
		{
			RaycastHit2D rayo = RayoHaciaElSuelo();
			// Una normal con y menor a 0,5 es una pared (más de ~60°): ahí no se pega
			if (rayo.collider != null && rayo.normal.y >= 0.5f)
			{
				// Para ir paralelo a la rampa, la velocidad tiene que ser perpendicular a la normal del suelo
				velocidadY = -velocidadX * rayo.normal.x / rayo.normal.y;
			}
		}

		rb.linearVelocity = new Vector2(velocidadX, velocidadY);
	}

	// La velocidad a la que quiere ir: la de correr, más lenta si está golpeado o en un puente, o la del lanzamiento
	private float CalcularVelocidadObjetivo()
	{
		if (isLanzado)
		{
			return velocidadLanzado;
		}

		float velocidad = runSpeed;
		if (golpeCoroutine != null)
		{
			velocidad = velocidad * multiplicadorVelocidadGolpe;
		}
		if (Time.time < lentoHasta)
		{
			velocidad = velocidad * multiplicadorVelocidadPuente;
		}
		return velocidad;
	}

	// Rayo hacia abajo que arranca un poco arriba de los pies y llega hasta distanciaPegado por debajo
	private RaycastHit2D RayoHaciaElSuelo()
	{
		Vector2 inicio = new Vector2(groundCheck.position.x, groundCheck.position.y + AlturaRayo);
		float largo = AlturaRayo + distanciaPegado;
		return Physics2D.Raycast(inicio, Vector2.down, largo, groundLayer);
	}

	private void AplicarSalto()
	{
		if (jumpRequested)
		{
			Saltar();
		}
		else if (isCorteSaltoPedido && isSaltoActivo && !isLanzado && rb.linearVelocity.y > 0f)
		{
			CortarSalto();
		}
		isCorteSaltoPedido = false;
	}

	private void Saltar()
	{
		// Saltar corta el deslizamiento y el aterrizaje
		if (isDeslizando)
		{
			TerminarDeslizamiento();
		}
		finAterrizaje = 0f;

		rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaPedida);
		jumpRequested = false;
		isSaltoActivo = true;
		alturaDespegue = rb.position.y;

		if (isLanzamientoPedido)
		{
			Sonido_fmod.Tocar(sonidoSuperSalto);
			isLanzamientoPedido = false;

			// Mientras sube atraviesa el suelo: le suma la capa del suelo a las que la cápsula ignora ("|" junta las dos máscaras).
			// Los triggers (como el agua) siguen avisando
			isAtravesando = true;
			capsula.excludeLayers = exclusionNormal | groundLayer;
		}
		else
		{
			Sonido_fmod.Tocar(sonidoSalto);
		}
	}

	// Soltó la tecla mientras subía: el salto termina en la primera altura (corta o media) que todavía no pasó.
	// Si ya pasó las dos, sigue el salto largo
	private void CortarSalto()
	{
		float subido = rb.position.y - alturaDespegue;

		float alturaObjetivo;
		if (subido < alturaSaltoCorto)
		{
			alturaObjetivo = alturaSaltoCorto;
		}
		else
		{
			alturaObjetivo = alturaSaltoMedio;
		}

		if (subido >= alturaObjetivo)
		{
			return;
		}

		// Física: para subir una altura h hace falta una velocidad v = raíz(2·g·h)
		float alturaQueFalta = alturaObjetivo - subido;
		float velocidadNecesaria = Mathf.Sqrt(2f * CalcularGravedad() * alturaQueFalta);

		// Solo frena: si ya va más lento, lo deja
		if (rb.linearVelocity.y > velocidadNecesaria)
		{
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, velocidadNecesaria);
		}
	}

	// Cayó por un hueco: baja por la pared sin fricción y sigue corriendo al tocar el suelo del carril de abajo
	private void ActualizarCaidaPorHueco()
	{
		if (isCayendoPedido && capsula != null)
		{
			isCayendoPedido = false;
			isCayendo = true;
			inicioCaida = Time.time;
			capsula.sharedMaterial = materialSinFriccion;
		}

		// Los primeros 0,2 s no cuentan: el isGrounded de antes de caer puede seguir en verdadero un instante
		if (isCayendo && isGrounded && Time.time > inicioCaida + 0.2f)
		{
			isCayendo = false;
			capsula.sharedMaterial = materialNormal;
		}
	}

	private void ActualizarLanzamiento()
	{
		// En el punto más alto del lanzamiento vuelve a chocar con el suelo
		if (isAtravesando && rb.linearVelocity.y <= 0f)
		{
			isAtravesando = false;
			capsula.excludeLayers = exclusionNormal;
		}

		// Termina al apoyarse. Los primeros 0,3 s no cuentan: antes de despegar todavía está apoyado
		if (isLanzado && !isAtravesando && isGrounded && Time.time > inicioLanzado + 0.3f)
		{
			isLanzado = false;
		}
	}

	private void ActualizarDeslizamiento()
	{
		if (slideRequested)
		{
			slideRequested = false;
			IniciarDeslizamiento();
		}

		// Termina al pasar el tiempo o si se cae del suelo
		if (isDeslizando && (Time.time >= finDeslizamiento || !isGrounded))
		{
			TerminarDeslizamiento();
		}
	}

	private void IniciarDeslizamiento()
	{
		isDeslizando = true;
		finAterrizaje = 0f;
		inicioDeslizamiento = Time.time;
		finDeslizamiento = Time.time + duracionDeslizamiento;
		SoltarPolvo(polvoDeslizamiento);
		Sonido_fmod.Tocar(sonidoBarrida);

		// Cápsula más baja. El centro baja la mitad de lo que se achicó, así los pies quedan en el mismo lugar
		float diferencia = capsulaSizeNormal.y - alturaDeslizando;
		capsula.size = new Vector2(capsulaSizeNormal.x, alturaDeslizando);
		capsula.offset = new Vector2(capsulaOffsetNormal.x, capsulaOffsetNormal.y - diferencia * 0.5f);
	}

	private void TerminarDeslizamiento()
	{
		isDeslizando = false;
		capsula.size = capsulaSizeNormal;
		capsula.offset = capsulaOffsetNormal;
	}

	private void SoltarPolvo(int cantidad)
	{
		if (polvo != null)
		{
			polvo.Emit(cantidad);
		}
	}

	// Gravedad que afecta al jugador: la del proyecto por su gravityScale
	private float CalcularGravedad()
	{
		return Physics2D.gravity.magnitude * rb.gravityScale;
	}

	// ---------- Lo que llaman otros scripts ----------

	// Lo llama cada recolectable al tocar al jugador
	public void Recolectar(Transform excusa)
	{
		Sonido_fmod.Tocar(sonidoRecolectar);
		if (Recolectado != null)
		{
			Recolectado(excusa);
		}
	}

	// Lo llama Puente_suelo en cada paso de física mientras se lo pisa
	public void Ralentizar()
	{
		// Recién empieza a pisar el puente: avisa (el cronómetro titila)
		if (Time.time >= lentoHasta && Ralentizado != null)
		{
			Ralentizado();
		}
		lentoHasta = Time.time + DuracionLentitud;
	}

	// Lo llama Alcantarilla: aparece en `destino` (dentro del caño de salida) y cae desde ahí a `velocidadCaida` hacia abajo
	public void Teletransportar(Vector2 destino, float velocidadCaida)
	{
		if (isMuerto)
		{
			return;
		}

		if (isDeslizando)
		{
			TerminarDeslizamiento();
		}
		jumpRequested = false;
		slideRequested = false;
		ultimoPedidoSalto = float.NegativeInfinity;
		ultimoEnSuelo = float.NegativeInfinity; // Sin esto, el coyote time dejaría saltar dentro del caño
		isSaltoActivo = false; // Así la caída se ve con el clip Fall y no con JumpUp
		finAterrizaje = 0f;

		rb.position = destino;
		// Sin esto, por la interpolación, el modelo se ve un cuadro a mitad de camino
		transform.position = new Vector3(destino.x, destino.y, transform.position.z);
		rb.linearVelocity = new Vector2(rb.linearVelocity.x, -velocidadCaida);
	}

	// Lo llama el hidrante. Sale disparado `altura` hacia arriba y hacia adelante a `velocidadX` hasta apoyarse de nuevo.
	// Mientras sube atraviesa el suelo, así no choca con el carril de arriba; en el punto más alto vuelve a ser sólido y cae sobre él
	public void Lanzar(float altura, float velocidadX)
	{
		if (isMuerto || capsula == null || altura <= 0f)
		{
			return;
		}

		// v = raíz(2·g·h): la velocidad hacia arriba con la que llega justo a esa altura
		fuerzaPedida = Mathf.Sqrt(2f * CalcularGravedad() * altura);
		jumpRequested = true;
		isLanzamientoPedido = true;
		isLanzado = true;
		velocidadLanzado = velocidadX;
		inicioLanzado = Time.time;
	}

	// Lo llama Zona_muerte al caer por un hueco del subterráneo
	public void Matar()
	{
		if (!isMuerto)
		{
			StartCoroutine(Morir());
		}
	}

	// ---------- Choques, golpe y muerte ----------

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (isMuerto)
		{
			return;
		}

		// Chocó de frente contra la pared de un tramo de suelo más alto
		if (EsParedDeSuelo(collision))
		{
			// En la calle y los edificios cae por el hueco al carril de abajo; en el subterráneo se muere
			Carril_suelo carrilSuelo = collision.gameObject.GetComponent<Carril_suelo>();
			if (carrilSuelo != null && carrilSuelo.Carril != Carril.Subterraneo)
			{
				isCayendoPedido = true;
			}
			else
			{
				StartCoroutine(Morir());
			}
			return;
		}

		// Chocó con un obstáculo: lo rompe y nos frena un rato
		Obstaculo obstaculo = collision.gameObject.GetComponent<Obstaculo>();
		if (obstaculo != null)
		{
			obstaculo.Romper();
			if (Golpeado != null)
			{
				Golpeado();
			}

			// Si chocamos otro antes de recuperarnos, el golpe vuelve a empezar
			if (golpeCoroutine != null)
			{
				StopCoroutine(golpeCoroutine);
			}
			golpeCoroutine = StartCoroutine(RecibirGolpe());
		}
	}

	// Pared = contacto casi horizontal con la capa de suelo, por delante del jugador (el piso y el techo tienen la normal vertical)
	private bool EsParedDeSuelo(Collision2D collision)
	{
		// groundLayer es una máscara: cada capa es un bit. Se fija si está prendido el bit de la capa del objeto
		int bitDeLaCapa = 1 << collision.gameObject.layer;
		bool isCapaDeSuelo = (groundLayer.value & bitDeLaCapa) != 0;
		if (!isCapaDeSuelo)
		{
			return false;
		}

		foreach (ContactPoint2D contacto in collision.contacts)
		{
			bool isNormalHorizontal = Mathf.Abs(contacto.normal.x) > 0.7f;
			bool isAdelante = contacto.point.x > transform.position.x;
			if (isNormalHorizontal && isAdelante)
			{
				return true;
			}
		}
		return false;
	}

	// Mientras dura esta corrutina el jugador va más lento (golpeCoroutine no es null) y titila
	private IEnumerator RecibirGolpe()
	{
		float fin = Time.time + duracionGolpe;
		bool isBlanco = false;
		while (Time.time < fin)
		{
			isBlanco = !isBlanco;
			PintarDestello(isBlanco);
			yield return Parpadeo;
		}

		PintarDestello(false);
		golpeCoroutine = null;
	}

	private IEnumerator Morir()
	{
		isMuerto = true;

		// Corta el golpe si estaba en curso (y con él su titileo)
		if (golpeCoroutine != null)
		{
			StopCoroutine(golpeCoroutine);
			golpeCoroutine = null;
		}
		if (isDeslizando)
		{
			TerminarDeslizamiento();
		}

		// Sin colliders cae por el hueco en vez de apoyarse en el borde
		Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
		foreach (Collider2D colisionador in colliders)
		{
			colisionador.enabled = false;
		}

		// Titila mientras cae
		float fin = Time.time + duracionMuerte;
		bool isBlanco = false;
		while (Time.time < fin)
		{
			isBlanco = !isBlanco;
			PintarDestello(isBlanco);
			yield return Parpadeo;
		}
		PintarDestello(false);

		pantallaGameOver.SetActive(true);
		Estado_juego.Congelar();
	}

	// Cambia el material de todas las partes del personaje: el destello blanco o el original de cada una
	private void PintarDestello(bool isBlanco)
	{
		for (int i = 0; i < renderizadores.Length; i++)
		{
			if (isBlanco)
			{
				renderizadores[i].sharedMaterial = materialDestello;
			}
			else
			{
				renderizadores[i].sharedMaterial = materialesNormales[i];
			}
		}
	}

	// ---------- Animaciones ----------

	// El código elige qué clip se ve; el Animator solo lo reproduce
	private void ActualizarAnimaciones()
	{
		// Deslizarse tiene prioridad sobre todo
		if (isDeslizando)
		{
			CambiarAnimacion(AnimSlide);
			return;
		}

		// Lanzado por el hidrante: JumpUp todo el vuelo (al atravesar el suelo isGrounded va y viene).
		// Ya bajando, cuando el sensor ve el suelo, arranca el aterrizaje como en un salto
		if (isLanzado)
		{
			if (Time.time >= finAterrizaje)
			{
				if (!isAtravesando && isPorAterrizar && puedeAterrizar)
				{
					EmpezarAterrizaje();
				}
				else
				{
					CambiarAnimacion(AnimJumpUp);
				}
			}

			// Sigue en el aire o terminando de aterrizar: no hace nada más
			if (isAtravesando || !isGrounded || Time.time < finAterrizaje)
			{
				return;
			}
		}

		if (isGrounded)
		{
			AnimarEnElSuelo();
		}
		else
		{
			AnimarEnElAire();
		}
	}

	private void AnimarEnElAire()
	{
		// El clip de aterrizaje ya arrancó antes de tocar el suelo: se deja terminar, salvo que vuelva a subir
		if (Time.time < finAterrizaje && rb.linearVelocity.y <= 0.1f)
		{
			return;
		}

		// Está por tocar el suelo: aterrizaje del salto si venía en JumpUp, o de la caída si venía en Fall
		if (isPorAterrizar && puedeAterrizar)
		{
			EmpezarAterrizaje();
			return;
		}

		// Se cayó de un borde sin saltar, o cae muy rápido: Fall. Si no, JumpUp de punta a punta del salto
		bool isCaidaLarga = !isSaltoActivo || rb.linearVelocity.y < -velocidadParaCaer;
		if (isCaidaLarga)
		{
			CambiarAnimacion(AnimFall);
		}
		else
		{
			CambiarAnimacion(AnimJumpUp);
		}
	}

	private void AnimarEnElSuelo()
	{
		// Aterrizaje en curso (arrancó antes de tocar el suelo): se deja terminar antes de volver a correr
		if (Time.time < finAterrizaje)
		{
			return;
		}

		// Tocó el suelo sin que el sensor lo viera a tiempo: aterrizaje igual
		if (puedeAterrizar && (animacionActual == AnimFall || animacionActual == AnimJumpUp))
		{
			EmpezarAterrizaje();
			return;
		}

		if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
		{
			CambiarAnimacion(AnimRun);
		}
		else
		{
			CambiarAnimacion(AnimIdle);
		}
	}

	private void EmpezarAterrizaje()
	{
		if (animacionActual == AnimJumpUp)
		{
			finAterrizaje = Time.time + duracionAterrizajeSalto;
			CambiarAnimacion(AnimJumpLand, inicioAterrizajeSalto);
		}
		else
		{
			finAterrizaje = Time.time + duracionAterrizajeCaida;
			CambiarAnimacion(AnimLand);
		}
	}

	// inicioSegundos: desde qué segundo del clip arranca (solo con mezcla). Si no se pasa, arranca desde el principio
	private void CambiarAnimacion(int nuevoHash, float inicioSegundos = 0f)
	{
		if (animacionActual == nuevoHash)
		{
			return;
		}

		animacionActual = nuevoHash;
		if (duracionTransicion > 0f)
		{
			animator.CrossFadeInFixedTime(nuevoHash, duracionTransicion, 0, inicioSegundos);
		}
		else
		{
			animator.Play(nuevoHash);
		}
	}

	void OnDrawGizmosSelected()
	{
		if (groundCheck != null)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
		}
	}
}
