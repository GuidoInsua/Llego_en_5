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

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
	}

	void Update()
	{
		// 1. Detección de suelo
		isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

		// 2. Velocidad horizontal constante
		rb.linearVelocity = new Vector2(runSpeed, rb.linearVelocity.y);

		// 3. Salto con el Nuevo Input System (Espacio o tecla W / Flecha arriba)
		bool jumpPressed = Keyboard.current != null &&
						  (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame);

		if (jumpPressed && isGrounded)
		{
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
		}

		// 4. Control de animaciones
		ActualizarAnimaciones();
	}

	private void ActualizarAnimaciones()
	{
		if (!isGrounded)
		{
			if (rb.linearVelocity.y > 0.1f)
			{
				CambiarAnimacion(AnimJumpUp, 0f);
			}
			else
			{
				CambiarAnimacion(AnimFall, 0f);
			}
		}
		else
		{
			if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
			{
				CambiarAnimacion(AnimRun, 0f);
			}
			else
			{
				CambiarAnimacion(AnimIdle, 0f);
			}
		}
	}

	public void CambiarAnimacion(int nuevoHash, float crossFade = 0f)
	{
		if (animacionActual == nuevoHash) return;

		animacionActual = nuevoHash;

		if (crossFade > 0f)
		{
			animator.CrossFade(nuevoHash, crossFade);
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
			Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
		}
	}
}