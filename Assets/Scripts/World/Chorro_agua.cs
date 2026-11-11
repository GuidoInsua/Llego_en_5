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
// Desc: Chorro de agua del hidrante: sube y baja, y ofrece el impulso con E si el jugador lo toca.

using UnityEngine;
using UnityEngine.InputSystem;

// Rectángulo azul (trigger) que crece y se achica en vertical. Mientras el jugador lo toca, y si no chocó con el hidrante,
// se ve el prompt de la tecla: si la aprieta en ese momento sale disparado hacia arriba (una sola vez); antes o después no hace nada
[RequireComponent(typeof(Collider2D))]
public class Chorro_agua : MonoBehaviour
{
	[Tooltip("La parte roja del hidrante. Si el jugador ya chocó con ella (o ya no está), el chorro no ofrece el impulso.")]
	[SerializeField] private Obstaculo cuerpo;

	[Header("Chorro")]
	[Tooltip("Alto del chorro cuando está más bajo y más alto (el sprite mide 1 de alto).")]
	[SerializeField] private Vector2 alto = new Vector2(1.5f, 4f);
	[SerializeField] private float periodo = 1.6f; // Segundos de un ciclo completo (sube y baja)

	[Header("Impulso")]
	[SerializeField] private Key tecla = Key.E;

	private Prompt_tecla prompt;
	private float fase; // Cada hidrante arranca en un punto distinto del ciclo
	private bool isUsado; // Ofrece el impulso una sola vez
	private Player_controller jugador; // El que está tocando el agua (null si no hay nadie)

	[Header("Lanzamiento")]
	[Tooltip("Cuánto sube el jugador por encima del hidrante (hasta el punto más alto). Se ajusta con los tramos de arriba para que caiga bien.")]
	[Range(5f, 100f)]
	[SerializeField] private float altura = 40f;
	[Tooltip("Velocidad horizontal del lanzamiento, en veces la de correr. Reemplaza a la de correr (no se suma).")]
	[SerializeField] private float multiplicadorVelocidad = 1.8f;

	void Awake()
	{
		prompt = FindAnyObjectByType<Prompt_tecla>();
		if (cuerpo == null || prompt == null || periodo <= 0f)
		{
			Debug.LogError("Chorro_agua: falta asignar el cuerpo, no hay un Prompt_tecla en la escena o el período no es mayor que 0.");
			enabled = false;
			return;
		}

		fase = Random.value * periodo;
	}

	void Update()
	{
		float t = (Mathf.Sin((Time.time + fase) / periodo * 2f * Mathf.PI) + 1f) * 0.5f;
		Vector3 escala = transform.localScale;
		escala.y = Mathf.Lerp(alto.x, alto.y, t);
		transform.localScale = escala;

		if (jugador == null) return;

		// Los choques de este paso de física ya se procesaron: si tocó la parte roja (rota o ya destruida), no hay impulso
		if (cuerpo == null || cuerpo.EstaRota || jugador.EstaMuerto)
		{
			Soltar();
			return;
		}

		// En pausa (timeScale 0) el input se ignora
		if (Time.timeScale > 0f && Keyboard.current != null && Keyboard.current[tecla].wasPressedThisFrame)
		{
			jugador.Lanzar(altura, jugador.VelocidadCarrera * multiplicadorVelocidad);
			isUsado = true;
			Soltar();
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (isUsado || cuerpo == null || cuerpo.EstaRota) return;

		Player_controller entrante = other.GetComponentInParent<Player_controller>();
		if (entrante == null) return;

		jugador = entrante;
		prompt.Mostrar(); // Si en este mismo paso chocó con la parte roja, Update lo oculta antes de dibujar
	}

	// Sale del agua (o el chorro se achica y deja de tocarlo): ya no hay impulso
	void OnTriggerExit2D(Collider2D other)
	{
		if (jugador != null && other.GetComponentInParent<Player_controller>() == jugador) Soltar();
	}

	private void Soltar()
	{
		jugador = null;
		prompt.Ocultar();
	}
}
