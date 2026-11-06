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

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Rectángulo azul (trigger) que crece y se achica en vertical. Si el jugador lo toca sin haber chocado antes con el hidrante,
// aparece el prompt de la tecla: si la aprieta enseguida sale disparado hacia arriba; si no, sigue corriendo
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
	[Tooltip("Segundos que tiene el jugador para apretar la tecla desde que toca el agua.")]
	[SerializeField] private float ventana = 0.8f;

	private Prompt_tecla prompt;
	private float fase; // Cada hidrante arranca en un punto distinto del ciclo
	private bool isUsado; // Ofrece el impulso una sola vez

	[Header("Lanzamiento")]
	[Tooltip("Cuánto sube el jugador por encima del hidrante (hasta el punto más alto). Se ajusta con los tramos de arriba para que caiga bien.")]
	[Range(5f, 100f)]
	[SerializeField] private float altura = 40f;
	[Tooltip("Velocidad horizontal del lanzamiento, en veces la de correr. Reemplaza a la de correr (no se suma).")]
	[SerializeField] private float multiplicadorVelocidad = 1.8f;

	void Awake()
	{
		prompt = FindFirstObjectByType<Prompt_tecla>();
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
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (isUsado) return;

		Player_controller jugador = other.GetComponentInParent<Player_controller>();
		if (jugador == null) return;

		isUsado = true;
		StartCoroutine(OfrecerImpulso(jugador));
	}

	private IEnumerator OfrecerImpulso(Player_controller jugador)
	{
		// Se espera un paso de física: si el choque con la parte roja ocurre en el mismo paso que el agua, ya hay que verlo roto
		yield return new WaitForFixedUpdate();

		// cuerpo == null: la parte roja ya se rompió y desapareció, o sea que el jugador chocó con ella
		if (cuerpo == null || cuerpo.EstaRota) yield break;

		prompt.Mostrar();

		float fin = Time.time + ventana;
		while (Time.time < fin && !jugador.EstaMuerto)
		{
			// En pausa (timeScale 0) el input se ignora
			if (Time.timeScale > 0f && Keyboard.current != null && Keyboard.current[tecla].wasPressedThisFrame)
			{
				jugador.Lanzar(altura, jugador.VelocidadCarrera * multiplicadorVelocidad);
				break;
			}
			yield return null;
		}

		prompt.Ocultar();
	}
}
