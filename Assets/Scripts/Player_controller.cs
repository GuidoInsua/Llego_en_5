using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // <-- 1. Importar el nuevo Input System

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(AudioSource))]
public class Player_controller : MonoBehaviour
{
	[Header("Movimiento")]
	[SerializeField] private float runSpeed = 7f;
	[SerializeField] private float jumpForce = 13f;

	[Header("Golpe")]
	[SerializeField] private float duracionGolpe = 1.5f;
	[SerializeField] private float multiplicadorVelocidadGolpe = 0.5f;

	[Header("Detección de Suelo")]
	[SerializeField] private Transform groundCheck;
	[SerializeField] private float checkRadius = 0.2f;
	[SerializeField] private LayerMask groundLayer;

	private static readonly int AnimIdle = Animator.StringToHash("Player_idle_anim");
	private static readonly int AnimRun = Animator.StringToHash("Player_run_anim");
	private static readonly int AnimJumpUp = Animator.StringToHash("Player_jumpUp_anim");
	private static readonly int AnimFall = Animator.StringToHash("Player_fall_anim");

	private Rigidbody2D rb;
	private Animator animator;
	private AudioSource pasosAudio;
	private SpriteRenderer spriteRenderer;
	private Coroutine golpeCoroutine;
	private int animacionActual;
	private bool isGrounded;
	private bool jumpRequested;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		pasosAudio = GetComponent<AudioSource>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		pasosAudio.loop = true; // Los pasos suenan en loop mientras dura la carrera
	}

	void Update()
	{
		// 1. Detección de suelo
		isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

		// 2. Salto con el Nuevo Input System (Espacio o tecla W / Flecha arriba)
		bool jumpPressed = Keyboard.current != null &&
						  (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame);

		if (jumpPressed && isGrounded)
		{
			jumpRequested = true;
		}

		// 3. Control de animaciones
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
		// Titila mostrando y ocultando el sprite hasta que pase el golpe
		float fin = Time.time + duracionGolpe;
		while (Time.time < fin)
		{
			spriteRenderer.enabled = !spriteRenderer.enabled;
			yield return new WaitForSeconds(0.1f);
		}

		spriteRenderer.enabled = true;
		golpeCoroutine = null;
	}

	private void ActualizarAnimaciones()
	{
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
			Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
		}
	}
}