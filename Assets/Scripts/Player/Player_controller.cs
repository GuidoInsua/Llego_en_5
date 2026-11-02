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

	[Header("Aire")]
	[Tooltip("Para un solo clip de salto: Jump cubre toda la subida y bajada (se queda en el último cuadro si dura más) y Fall solo aparece en caídas largas o al caer sin saltar.")]
	[SerializeField] private bool mantenerSaltoEnDescenso;
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

	[Header("Muerte")]
	[Tooltip("Chocar de frente contra el suelo (la pared de un tramo más alto): titila y cae por el hueco este tiempo, y después aparece el game over.")]
	[SerializeField] private float duracionMuerte = 1.5f;
	[Tooltip("Empieza apagada; se prende al terminar de caer.")]
	[SerializeField] private GameObject pantallaGameOver;

	[SerializeField] private Material materialDestello; // Pinta el personaje entero de blanco (shader GUI/Text Shader)

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
	private float inicioDeslizamiento;
	private float ultimoPedidoDeslizar = float.NegativeInfinity;
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
		renderizadores = GetComponentsInChildren<Renderer>();

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

		capsula = GetComponent<CapsuleCollider2D>();
		puedeAterrizar = animator.HasState(0, AnimLand) && animator.HasState(0, AnimJumpLand);
		puedeDeslizar = capsula != null && animator.HasState(0, AnimSlide);
		if (capsula != null)
		{
			capsulaSizeNormal = capsula.size;
			capsulaOffsetNormal = capsula.offset;
		}
	}

	// Lo escucha Cronometro para avisar que se pierde tiempo
	public event System.Action Golpeado;

	// La cámara usa esto para saber cuándo el jugador apoyó los pies
	public bool EstaEnSuelo
	{
		get { return isGrounded; }
	}

	void Update()
	{
		// 1. Detección de suelo
		isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

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
		bool jumpPressed = Time.timeScale > 0f && Keyboard.current != null &&
						  (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame);

		if (jumpPressed && isGrounded && (!isDeslizando || Time.time >= inicioDeslizamiento + tiempoMinimoParaSaltar))
		{
			jumpRequested = true;
		}

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

		// Física: la velocidad se aplica en FixedUpdate según las reglas del proyecto
		float velocidadX = golpeCoroutine != null ? runSpeed * multiplicadorVelocidadGolpe : runSpeed;
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
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
			jumpRequested = false;
			isSaltoActivo = true;
			finAterrizaje = 0f; // Saltar corta el aterrizaje
		}

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

		// Cápsula más baja con los pies en el mismo lugar
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

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (isMuerto) return;

		// Chocar de frente contra el suelo: el avance quedaría trabado contra la pared
		if (EsParedDeSuelo(collision))
		{
			StartCoroutine(Morir());
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

	private IEnumerator Morir()
	{
		isMuerto = true;
		if (golpeCoroutine != null) StopCoroutine(golpeCoroutine);
		golpeCoroutine = null;
		if (isDeslizando) TerminarDeslizamiento();

		// Sin colliders cae por el hueco en vez de apoyarse en el borde
		foreach (Collider2D c in GetComponentsInChildren<Collider2D>()) c.enabled = false;

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
		Time.timeScale = 0f;
		AudioListener.pause = true;
	}

	private IEnumerator RecibirGolpe()
	{
		// Titila alternando el modelo entre normal y blanco hasta que pase el golpe
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

		if (!isGrounded && mantenerSaltoEnDescenso)
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
		else if (!isGrounded)
		{
			if (rb.linearVelocity.y > 0.1f)
			{
				CambiarAnimacion(AnimJumpUp);
			}
			else
			{
				CambiarAnimacion(AnimFall);
			}
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