using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(AudioSource))] // El Animator y los renderers pueden estar en un hijo (modelo 3D)
public class Player_controller : MonoBehaviour
{
	[Header("Movimiento")]
	[SerializeField] private float runSpeed = 7f;
	[SerializeField] private float jumpForce = 13f;

	[Header("Deslizarse")]
	[SerializeField] private float duracionDeslizamiento = 1.2f;
	[SerializeField] private float alturaDeslizando = 0.8f; // Alto de la cápsula mientras se desliza: los pies quedan en el mismo lugar

	[Header("Golpe")]
	[SerializeField] private float duracionGolpe = 1.5f;
	[SerializeField] private float multiplicadorVelocidadGolpe = 0.5f;

	[SerializeField] private Material materialDestello; // Pinta el personaje entero de blanco (shader GUI/Text Shader)

	[Header("Detección de Suelo")]
	[SerializeField] private Transform groundCheck;
	[SerializeField] private Vector2 groundCheckSize = new Vector2(0.15f, 0.05f); // Más angosta que los pies para no detectar paredes
	[SerializeField] private LayerMask groundLayer;

	private static readonly WaitForSeconds Parpadeo = new WaitForSeconds(0.1f); // Se reutiliza: evita crear uno nuevo por cada titilada
	private static readonly int AnimIdle= Animator.StringToHash("Player_idle_anim");
	private static readonly int AnimRun = Animator.StringToHash("Player_run_anim");
	private static readonly int AnimJumpUp = Animator.StringToHash("Player_jumpUp_anim");
	private static readonly int AnimFall = Animator.StringToHash("Player_fall_anim");
	private static readonly int AnimSlide = Animator.StringToHash("Player_slide_anim");

	private Rigidbody2D rb;
	private Animator animator;
	private AudioSource pasosAudio;
	private Renderer[] renderizadores;
	private Material[] materialesNormales; // Uno por renderer, en el mismo orden
	private CapsuleCollider2D capsula;
	private Vector2 capsulaSizeNormal;
	private Vector2 capsulaOffsetNormal;
	private Coroutine golpeCoroutine;
	private float finDeslizamiento;
	private int animacionActual;
	private bool isGrounded;
	private bool jumpRequested;
	private bool slideRequested;
	private bool isDeslizando;
	private bool puedeDeslizar; // Solo si hay cápsula y el Animator tiene el clip: el jugador de sprites no lo tiene

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponentInChildren<Animator>();
		pasosAudio = GetComponent<AudioSource>();
		renderizadores = GetComponentsInChildren<Renderer>();

		if (groundCheck == null || materialDestello == null || animator == null || renderizadores.Length == 0)
		{
			Debug.LogError("Player_controller: falta asignar groundCheck o materialDestello, o no hay Animator ni Renderer en el jugador o sus hijos.");
			enabled = false;
			return;
		}

		materialesNormales = new Material[renderizadores.Length];
		for (int i = 0; i < renderizadores.Length; i++)
		{
			materialesNormales[i] = renderizadores[i].sharedMaterial;
		}

		capsula = GetComponent<CapsuleCollider2D>();
		puedeDeslizar = capsula != null && animator.HasState(0, AnimSlide);
		if (capsula != null)
		{
			capsulaSizeNormal = capsula.size;
			capsulaOffsetNormal = capsula.offset;
		}

		pasosAudio.loop = true; // Los pasos suenan en loop mientras dura la carrera
		pasosAudio.volume *= Volumen_audio.Efectos; // Los pasos no pasan por Efectos_sonido: aplican el volumen acá
	}

	void Update()
	{
		// 1. Detección de suelo
		isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

		// 2. Salto con el Nuevo Input System (Espacio o tecla W / Flecha arriba)
		// En pausa (timeScale 0) el input se ignora
		bool jumpPressed = Time.timeScale > 0f && Keyboard.current != null &&
						  (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame);

		if (jumpPressed && isGrounded && !isDeslizando)
		{
			jumpRequested = true;
		}

		// 3. Deslizarse con Shift, solo desde el suelo
		bool slidePressed = Time.timeScale > 0f && Keyboard.current != null &&
						   (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame);

		if (slidePressed && puedeDeslizar && isGrounded && !isDeslizando)
		{
			slideRequested = true;
		}

		// 4. Control de animaciones
		ActualizarAnimaciones();
	}

	void FixedUpdate()
	{
		// Física: la velocidad se aplica en FixedUpdate según las reglas del proyecto
		rb.linearVelocity = new Vector2(golpeCoroutine != null ? runSpeed * multiplicadorVelocidadGolpe : runSpeed, rb.linearVelocity.y);

		if (jumpRequested)
		{
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
			jumpRequested = false;
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

	private void IniciarDeslizamiento()
	{
		isDeslizando = true;
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
		// Chocar con una caja: la rompe y nos frena un rato
		if (collision.gameObject.TryGetComponent(out Obstaculo caja))
		{
			caja.Romper();

			// Si chocamos otra caja antes de recuperarnos, reiniciamos el efecto
			if (golpeCoroutine != null) StopCoroutine(golpeCoroutine);
			golpeCoroutine = StartCoroutine(RecibirGolpe());
		}
	}

	private IEnumerator RecibirGolpe()
	{
		// Titila alternando el sprite entre normal y blanco hasta que pase el golpe
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

		if (!isGrounded)
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

	private void CambiarAnimacion(int nuevoHash)
	{
		if (animacionActual == nuevoHash) return;

		animacionActual = nuevoHash;
		animator.Play(nuevoHash);

		// Solo al cambiar de estado: los pasos suenan únicamente corriendo en el suelo
		if (nuevoHash == AnimRun) pasosAudio.Play();
		else pasosAudio.Stop();
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