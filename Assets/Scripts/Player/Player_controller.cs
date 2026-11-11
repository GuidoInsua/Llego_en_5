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
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))] // El Animator y los renderers pueden estar en un hijo (modelo 3D)
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
	[Tooltip("Al soltar la tecla mientras sube, la velocidad vertical se multiplica por esto (salto más bajo). 1 = salto siempre completo.")]
	[SerializeField, Range(0f, 1f)] private float multiplicadorCorteSalto = 0.5f;

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

	[Header("Detección de Suelo")]
	[SerializeField] private Transform groundCheck;
	[SerializeField] private Vector2 groundCheckSize = new Vector2(0.15f, 0.05f); // Más angosta que los pies para no detectar paredes
	[SerializeField] private LayerMask groundLayer;
	[Tooltip("Hasta qué distancia bajo los pies el jugador se pega al suelo en las pendientes. Más que eso se cae.")]
	[SerializeField] private float distanciaPegado = 0.4f;
	[Tooltip("Si cae más rápido que esto no se pega: las caídas grandes no se frenan antes de tocar el suelo.")]
	[SerializeField] private float velocidadMaximaPegado = 8f;

	private static readonly WaitForSeconds Parpadeo = new WaitForSeconds(0.1f); // Se reutiliza: evita crear uno nuevo por cada titilada
	private static readonly int AnimIdle= Animator.StringToHash("Player_idle_anim");
	private static readonly int AnimRun = Animator.StringToHash("Player_run_anim");
	private static readonly int AnimJumpUp = Animator.StringToHash("Player_jumpUp_anim");
	private static readonly int AnimFall = Animator.StringToHash("Player_fall_anim");
	private static readonly int AnimSlide = Animator.StringToHash("Player_slide_anim");
	private static readonly int AnimLand = Animator.StringToHash("Player_land_anim");
	private static readonly int AnimJumpLand = Animator.StringToHash("Player_jumpLand_anim");

	private Rigidbody2D rb;
	private Animator animator;
	private Renderer[] renderizadores;
	private Material[] materialesNormales; // Uno por renderer, en el mismo orden
	private CapsuleCollider2D capsula;
	private Vector2 capsulaSizeNormal;
	private Vector2 capsulaOffsetNormal;
	private Coroutine golpeCoroutine;
	private bool isLanzamientoPedido; // Lanzar() lo pide y FixedUpdate lo aplica
	private bool isLanzado; // Desde el lanzamiento hasta volver a apoyarse: corre a velocidadLanzado
	private float velocidadLanzado;
	private float inicioLanzado;
	private bool isAtravesando; // Sube sin chocar con nada hasta el punto más alto
	private const float DuracionLentitud = 0.1f;
	private float lentoHasta;
	private bool isCayendoPedido; // Cayó por un hueco de la calle o los edificios (no del subterráneo): OnCollisionEnter2D lo pide y FixedUpdate lo aplica
	public event System.Action<Carril> CarrilCambiado; // Al pisar un suelo de otro carril
	private Carril? carrilActual; // null hasta pisar el primer suelo
	private bool isCayendo;// Cae pegado a la pared sin fricción hasta volver a tocar el suelo
	private float inicioCaida;
	private PhysicsMaterial2D materialNormal;
	private PhysicsMaterial2D materialSinFriccion; // Sin esto, empujando contra la pared (la x es constante) se queda pegado en vez de caer
	private LayerMask exclusionNormal;
	private float fuerzaPedida; // Velocidad vertical del próximo salto: jumpForce, o la del lanzamiento
	private float inicioDeslizamiento;
	private float ultimoPedidoDeslizar = float.NegativeInfinity;
	private float ultimoPedidoSalto = float.NegativeInfinity;
	private float ultimoEnSuelo = float.NegativeInfinity;
	private bool isCorteSaltoPedido; // Soltó la tecla de salto mientras subía: FixedUpdate baja la velocidad
	private float velocidadActual; // Velocidad horizontal: frena de golpe y acelera de a poco
	private float velocidadCaida; // La más rápida de esta caída, para el polvo y la cámara al aterrizar
	private Collider2D ultimoSuelo; // Para buscar Carril_suelo solo al cambiar de suelo
	private float finDeslizamiento;
	private float finAterrizaje;
	private int animacionActual;
	private bool isGrounded;
	private bool jumpRequested;
	private bool slideRequested;
	private bool isPorAterrizar; // El sensor ve el suelo (no un obstáculo) justo debajo de los pies
	private bool isSaltoActivo; // Verdadero desde que despega con un salto hasta que vuelve a tocar el suelo
	private bool isDeslizando;
	private bool isMuerto;
	private bool puedeAterrizar; // Solo si el Animator tiene los states de aterrizaje
	private bool puedeDeslizar; // Solo si hay cápsula y el Animator tiene el clip

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponentInChildren<Animator>();
		// Solo los del modelo (bajo el Animator): las partículas del polvo no titilan
		renderizadores = animator != null ? animator.GetComponentsInChildren<Renderer>() : new Renderer[0];

		if (groundCheck == null || materialDestello == null || pantallaGameOver == null || animator == null || renderizadores.Length == 0)
		{
			Debug.LogError("Player_controller: falta asignar groundCheck, materialDestello o pantallaGameOver, o no hay Animator ni Renderer en el jugador o sus hijos.");
			enabled = false;
			return;
		}

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
			materialSinFriccion = new PhysicsMaterial2D("SinFriccion") { friction = 0f, bounciness = 0f };
		}
	}

	// Lo escucha Cronometro para avisar que se pierde tiempo
	public event System.Action Golpeado;
	public event System.Action Recolectado;
	// Al tocar el suelo después de estar en el aire, con la velocidad de caída más alta (positiva). Lo escucha Sacudida_camara
	public event System.Action<float> Aterrizado;

	// Lo llama cada recolectable al tocar al jugador
	public void Recolectar() => Recolectado?.Invoke();

	// Lo usa Alcantarilla: entra solo el que empieza a deslizarse encima de la tapa
	public bool EstaDeslizando
	{
		get { return isDeslizando; }
	}

	// Lo usa el hidrante para dejar de ofrecer el impulso si el jugador ya murió
	public bool EstaMuerto
	{
		get { return isMuerto; }
	}

	private float VelocidadHorizontal
	{
		get
		{
			if (isLanzado) return velocidadLanzado;
			float velocidad = golpeCoroutine != null ? runSpeed * multiplicadorVelocidadGolpe : runSpeed;
			return Time.time < lentoHasta ? velocidad * multiplicadorVelocidadPuente : velocidad;
		}
	}

	// Lo llama Puente_suelo en cada paso de física mientras se lo pisa: la lentitud dura un instante más para no titilar
	public void Ralentizar()
	{
		lentoHasta = Time.time + DuracionLentitud;
	}

	// Lo llama Alcantarilla (desde la física): aparece en `destino`, dentro del caño de salida, y cae desde ahí, a `velocidadCaida` hacia abajo, con la animación de caída
	public void Teletransportar(Vector2 destino, float velocidadCaida)
	{
		if (isMuerto) return;

		if (isDeslizando) TerminarDeslizamiento();
		jumpRequested = false;
		slideRequested = false;
		ultimoPedidoSalto = float.NegativeInfinity;
		ultimoEnSuelo = float.NegativeInfinity; // Sin esto el coyote time dejaría saltar dentro del caño
		isSaltoActivo = false; // Así la caída es Fall y no JumpUp
		finAterrizaje = 0f;
		rb.position = destino;
		transform.position = new Vector3(destino.x, destino.y, transform.position.z); // Sin esto, con la interpolación el modelo se ve un cuadro a mitad de camino
		rb.linearVelocity = new Vector2(rb.linearVelocity.x, -velocidadCaida);
	}

	// Lo usa el hidrante para fijar la velocidad horizontal del lanzamiento
	public float VelocidadCarrera
	{
		get { return runSpeed; }
	}

	// Sale disparado hacia arriba `altura` por encima de donde está y hacia adelante a `velocidadX` hasta apoyarse de nuevo (como un jump pad). Mientras sube atraviesa el suelo:
	// así no se choca con el carril de arriba; al llegar al punto más alto vuelve a ser sólido y cae sobre ese suelo. Es un salto con otra fuerza
	public void Lanzar(float altura, float velocidadX)
	{
		if (isMuerto || capsula == null || altura <= 0f) return;

		float gravedad = Physics2D.gravity.magnitude * rb.gravityScale;
		fuerzaPedida = Mathf.Sqrt(2f * gravedad * altura);
		jumpRequested = true;
		isLanzamientoPedido = true;
		isLanzado = true;
		velocidadLanzado = velocidadX;
		inicioLanzado = Time.time;
	}

	// La cámara usa esto para saber cuándo el jugador apoyó los pies
	public bool EstaEnSuelo
	{
		get { return isGrounded; }
	}

	void Update()
	{
		// 1. Detección de suelo
		Collider2D suelo = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
		bool estabaEnSuelo = isGrounded;
		isGrounded = suelo != null && !isAtravesando; // OverlapBox no respeta el excludeLayers: al cruzar el suelo en el lanzamiento no cuenta como apoyado

		if (isGrounded)
		{
			ultimoEnSuelo = Time.time;

			if (suelo != ultimoSuelo)
			{
				ultimoSuelo = suelo;
				if (suelo.TryGetComponent(out Carril_suelo carrilSuelo) && carrilSuelo.Carril != carrilActual)
				{
					carrilActual = carrilSuelo.Carril;
					CarrilCambiado?.Invoke(carrilSuelo.Carril);
				}
			}

			if (!estabaEnSuelo && !isMuerto)
			{
				if (velocidadCaida >= velocidadMinimaPolvo) SoltarPolvo(polvoAterrizaje);
				Aterrizado?.Invoke(velocidadCaida);
			}
			velocidadCaida = 0f;
		}
		else
		{
			velocidadCaida = Mathf.Max(velocidadCaida, -rb.linearVelocity.y);
		}

		// Sensor de aterrizaje: solo mira la capa Ground, así un obstáculo en el camino no lo activa
		isPorAterrizar = false;
		if (!isGrounded && rb.linearVelocity.y < -0.5f)
		{
			// Distancia que recorre cayendo en el tiempo de anticipo, con la aceleración de la gravedad
			float gravedad = Physics2D.gravity.magnitude * rb.gravityScale;
			float caida = -rb.linearVelocity.y * anticipoAterrizaje + 0.5f * gravedad * anticipoAterrizaje * anticipoAterrizaje;
			float distancia = Mathf.Max(distanciaMinimaAterrizaje, caida);
			isPorAterrizar = Physics2D.BoxCast(groundCheck.position, groundCheckSize, 0f, Vector2.down, distancia, groundLayer);
		}

		// 2. Salto con el Nuevo Input System (Espacio o tecla W / Flecha arriba)
		// En pausa (timeScale 0) el input se ignora
		// El pedido se guarda `bufferSalto` s (salta al aterrizar) y vale hasta `tiempoCoyote` s después de dejar un borde, si no saltó
		bool isTeclado = Time.timeScale > 0f && Keyboard.current != null;
		bool jumpPressed = isTeclado &&
						  (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame);
		bool jumpReleased = isTeclado &&
						   (Keyboard.current.spaceKey.wasReleasedThisFrame || Keyboard.current.wKey.wasReleasedThisFrame || Keyboard.current.upArrowKey.wasReleasedThisFrame);

		if (jumpPressed) ultimoPedidoSalto = Time.time;

		bool isApoyado = isGrounded || (!isSaltoActivo && !isAtravesando && Time.time - ultimoEnSuelo <= tiempoCoyote);
		if (isApoyado && Time.time - ultimoPedidoSalto <= bufferSalto && (!isDeslizando || Time.time >= inicioDeslizamiento + tiempoMinimoParaSaltar))
		{
			if (!jumpRequested) fuerzaPedida = jumpForce; // Si ya hay un lanzamiento pedido, no se pisa
			jumpRequested = true;
			ultimoPedidoSalto = float.NegativeInfinity;
		}

		// Soltar después del despegue: si se soltó antes (salto guardado en el buffer), el salto es completo
		if (jumpReleased && isSaltoActivo && !isLanzado) isCorteSaltoPedido = true;

		if (isGrounded && rb.linearVelocity.y <= 0.1f)
		{
			isSaltoActivo = false;
		}

		// 3. Deslizarse con Shift, flecha abajo o S, solo desde el suelo (el pedido se guarda `bufferDeslizar` s si se aprieta justo antes de aterrizar)
		bool slidePressed = Time.timeScale > 0f && Keyboard.current != null &&
						   (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame ||
							Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame);

		if (slidePressed) ultimoPedidoDeslizar = Time.time;

		if (puedeDeslizar && isGrounded && !isDeslizando && Time.time - ultimoPedidoDeslizar <= bufferDeslizar)
		{
			slideRequested = true;
			ultimoPedidoDeslizar = float.NegativeInfinity;
		}

		// 4. Control de animaciones
		ActualizarAnimaciones();
		animator.speed = animacionActual == AnimRun && velocidadClipCorrer > 0f ? Mathf.Abs(rb.linearVelocity.x) / velocidadClipCorrer : 1f;
	}

	void FixedUpdate()
	{
		// Muerto: queda quieto en x y cae por la gravedad
		if (isMuerto)
		{
			rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
			return;
		}

		// Física: la velocidad se aplica en FixedUpdate según las reglas del proyecto.
		// Frenar (golpe, puente) y el lanzamiento son instantáneos; volver a la velocidad normal es gradual
		float objetivo = VelocidadHorizontal;
		velocidadActual = isLanzado || objetivo < velocidadActual ? objetivo : Mathf.MoveTowards(velocidadActual, objetivo, aceleracion * Time.fixedDeltaTime);
		float velocidadX = velocidadActual;
		float velocidadY = rb.linearVelocity.y;

		// En una pendiente la velocidad sigue el suelo: sin esto, al bajar una rampa se despega y flota, y al subir se frena contra ella
		if (!jumpRequested && !isSaltoActivo && velocidadY > -velocidadMaximaPegado && NormalDelSuelo(out Vector2 normal))
		{
			velocidadY = -velocidadX * normal.x / normal.y;
		}

		rb.linearVelocity = new Vector2(velocidadX, velocidadY);

		if (jumpRequested)
		{
			if (isDeslizando) TerminarDeslizamiento(); // Saltar corta el deslizamiento
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaPedida);
			jumpRequested = false;
			isSaltoActivo = true;
			finAterrizaje = 0f; // Saltar corta el aterrizaje

			if (isLanzamientoPedido)
			{
				isLanzamientoPedido = false;
				isAtravesando = true;
				capsula.excludeLayers = exclusionNormal | groundLayer; // Solo el suelo: los triggers (zonas de tramo, agua) siguen avisando
			}
		}
		else if (isCorteSaltoPedido && isSaltoActivo && !isLanzado && rb.linearVelocity.y > 0f)
		{
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * multiplicadorCorteSalto);
		}
		isCorteSaltoPedido = false;

		// Cayó por un hueco: baja por la pared sin fricción y sigue corriendo al tocar el suelo del carril de abajo
		if (isCayendoPedido && capsula != null)
		{
			isCayendoPedido = false;
			isCayendo = true;
			inicioCaida = Time.time;
			capsula.sharedMaterial = materialSinFriccion;
		}

		if (isCayendo && isGrounded && Time.time > inicioCaida + 0.2f) // El isGrounded de antes de caer puede seguir en verdadero un instante
		{
			isCayendo = false;
			capsula.sharedMaterial = materialNormal;
		}

		// En el punto más alto del lanzamiento vuelve a chocar con todo
		if (isAtravesando && rb.linearVelocity.y <= 0f)
		{
			isAtravesando = false;
			capsula.excludeLayers = exclusionNormal;
		}

		// El 0,3 s evita cortarlo antes del despegue, cuando todavía está apoyado
		if (isLanzado && !isAtravesando && isGrounded && Time.time > inicioLanzado + 0.3f) isLanzado = false;

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

	// Normal del suelo bajo los pies si está a menos de distanciaPegado y no es una pared (más de ~60° de pendiente)
	private bool NormalDelSuelo(out Vector2 normal)
	{
		const float AlturaRayo = 0.1f; // El rayo arranca un poco arriba de los pies
		RaycastHit2D golpe = Physics2D.Raycast((Vector2)groundCheck.position + Vector2.up * AlturaRayo, Vector2.down, AlturaRayo + distanciaPegado, groundLayer);
		normal = golpe.normal;
		return golpe.collider != null && golpe.normal.y >= 0.5f;
	}

	private void IniciarDeslizamiento()
	{
		isDeslizando = true;
		finAterrizaje = 0f;
		inicioDeslizamiento = Time.time;
		finDeslizamiento = Time.time + duracionDeslizamiento;
		SoltarPolvo(polvoDeslizamiento);

		// Cápsula más baja con los pies en el mismo lugar
		float diferencia = capsulaSizeNormal.y - alturaDeslizando;
		capsula.size = new Vector2(capsulaSizeNormal.x, alturaDeslizando);
		capsula.offset = new Vector2(capsulaOffsetNormal.x, capsulaOffsetNormal.y - diferencia * 0.5f);
	}

	private void SoltarPolvo(int cantidad)
	{
		if (polvo != null) polvo.Emit(cantidad);
	}

	private void TerminarDeslizamiento()
	{
		isDeslizando = false;
		capsula.size = capsulaSizeNormal;
		capsula.offset = capsulaOffsetNormal;
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (isMuerto) return;

		// Chocar de frente contra el suelo: el avance quedaría trabado contra la pared
		if (EsParedDeSuelo(collision))
		{
			// Solo se muere en el subterráneo: en la calle o los edificios es caer por el hueco al carril de abajo
			if (collision.gameObject.TryGetComponent(out Carril_suelo carril) && carril.Carril != Carril.Subterraneo) isCayendoPedido = true;
			else StartCoroutine(Morir());
			return;
		}

		// Chocar con una caja: la rompe y nos frena un rato
		if (collision.gameObject.TryGetComponent(out Obstaculo caja))
		{
			caja.Romper();
			Golpeado?.Invoke();

			// Si chocamos otra caja antes de recuperarnos, reiniciamos el efecto
			if (golpeCoroutine != null) StopCoroutine(golpeCoroutine);
			golpeCoroutine = StartCoroutine(RecibirGolpe());
		}
	}

	// Pared = contacto casi horizontal con la capa de suelo, por delante del jugador (el piso y el techo tienen la normal vertical)
	private bool EsParedDeSuelo(Collision2D collision)
	{
		if ((groundLayer.value & (1 << collision.gameObject.layer)) == 0) return false;

		foreach (ContactPoint2D contacto in collision.contacts)
		{
			if (Mathf.Abs(contacto.normal.x) > 0.7f && contacto.point.x > transform.position.x) return true;
		}
		return false;
	}

	// Lo llama Zona_muerte al caer por un hueco del subterráneo
	public void Matar()
	{
		if (!isMuerto) StartCoroutine(Morir());
	}

	private IEnumerator Morir()
	{
		isMuerto = true;
		if (golpeCoroutine != null) StopCoroutine(golpeCoroutine);
		golpeCoroutine = null;
		if (isDeslizando) TerminarDeslizamiento();

		// Sin colliders cae por el hueco en vez de apoyarse en el borde
		foreach (Collider2D c in GetComponentsInChildren<Collider2D>()) c.enabled = false;

		IEnumerator titileo = Titilar(duracionMuerte);
		while (titileo.MoveNext()) yield return titileo.Current;
		pantallaGameOver.SetActive(true);
		Estado_juego.Congelar();
	}

	private IEnumerator RecibirGolpe()
	{
		IEnumerator titileo = Titilar(duracionGolpe);
		while (titileo.MoveNext()) yield return titileo.Current;
		golpeCoroutine = null;
	}

	// Alterna el modelo entre normal y blanco durante `duracion` y lo deja normal.
	// Se recorre a mano y no con `yield return Titilar(...)`: así, un StopCoroutine del golpe también corta el titileo
	private IEnumerator Titilar(float duracion)
	{
		float fin = Time.time + duracion;
		bool isBlanco = false;
		while (Time.time < fin)
		{
			isBlanco = !isBlanco;
			PintarDestello(isBlanco);
			yield return Parpadeo;
		}

		PintarDestello(false);
	}

	// Cambia el material de todas las partes del personaje: el destello blanco o el original de cada una
	private void PintarDestello(bool isBlanco)
	{
		for (int i = 0; i < renderizadores.Length; i++)
		{
			renderizadores[i].sharedMaterial = isBlanco ? materialDestello : materialesNormales[i];
		}
	}

	private void ActualizarAnimaciones()
	{
		if (isDeslizando)
		{
			CambiarAnimacion(AnimSlide);
			return;
		}

		// Lanzado por el hidrante: JumpUp todo el vuelo (al subir atraviesa el suelo y isGrounded parpadea). Ya bajando, el sensor arranca el aterrizaje
		// como en un salto: antes había un tiempo fijo desde que pasaba por la zona de un tramo de arriba y, si subía de más, aterrizaba en el aire
		if (isLanzado)
		{
			if (!isAtravesando && isPorAterrizar && puedeAterrizar && Time.time >= finAterrizaje)
			{
				EmpezarAterrizaje();
			}
			else if (Time.time >= finAterrizaje)
			{
				CambiarAnimacion(AnimJumpUp);
			}

			if (isAtravesando || !isGrounded || Time.time < finAterrizaje) return;
		}

		if (!isGrounded)
		{
			// El clip de aterrizaje ya arrancó antes de tocar el suelo: se deja terminar, salvo que vuelva a subir
			if (Time.time < finAterrizaje && rb.linearVelocity.y <= 0.1f)
			{
				return;
			}

			// Por tocar el suelo: aterrizaje del salto si venía en JumpUp, o de la caída si venía en Fall
			if (isPorAterrizar && puedeAterrizar)
			{
				EmpezarAterrizaje();
				return;
			}

			// Sin saltar (se cayó de un borde) o cayendo rápido: Fall. Si no, JumpUp sigue de punta a punta
			bool isCaidaLarga = !isSaltoActivo || rb.linearVelocity.y < -velocidadParaCaer;
			CambiarAnimacion(isCaidaLarga ? AnimFall : AnimJumpUp);
		}
		else
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

	// inicioSegundos: desde qué segundo del clip arranca (solo con mezcla)
	private void CambiarAnimacion(int nuevoHash, float inicioSegundos = 0f)
	{
		if (animacionActual == nuevoHash) return;

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