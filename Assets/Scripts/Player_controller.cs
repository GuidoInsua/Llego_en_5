using UnityEngine;
using UnityEngine.InputSystem; // <-- 1. Importar el nuevo Input System

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Player_controller : MonoBehaviour
{
	[Header("Movimiento")]
	[SerializeField] private float runSpeed = 7f;
	[SerializeField] private float jumpForce = 13f;

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
	private int animacionActual;
	private bool isGrounded;
	private bool jumpRequested;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
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
		rb.linearVelocity = new Vector2(runSpeed, rb.linearVelocity.y);

		if (jumpRequested)
		{
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
			jumpRequested = false;
		}
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