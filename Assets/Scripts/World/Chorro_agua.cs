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

// Rectángulo azul (trigger) que crece y se achica en vertical. Mientras el jugador lo toca, y si no chocó con el hidrante,
// si aprieta interactuar (E o Espacio) sale disparado hacia arriba (una sola vez); antes o después no hace nada.
// La tecla se ve siempre en el prefab (hijo TeclaE)
[RequireComponent(typeof(Collider2D))]
public class Chorro_agua : MonoBehaviour
{
	[Tooltip("La parte roja del hidrante. Si el jugador ya chocó con ella, el chorro no ofrece el impulso.")]
	[SerializeField] private Obstaculo cuerpo;

	[Header("Chorro")]
	[Tooltip("Alto del chorro cuando está más bajo y más alto (el sprite mide 1 de alto).")]
	[SerializeField] private Vector2 alto = new Vector2(1.5f, 4f);
	[SerializeField] private float periodo = 1.6f; // Segundos de un ciclo completo (sube y baja)

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
		if (cuerpo == null || periodo <= 0f)
		{
			Debug.LogError("Chorro_agua: falta asignar el cuerpo o el período no es mayor que 0.");
			enabled = false;
			return;
		}

		fase = Random.value * periodo;
	}

	void Update()
	{
		// Onda que va de 0 a 1 y vuelve, un ciclo completo cada `periodo` segundos.
		// El seno va de -1 a 1: sumarle 1 y dividir por 2 lo pasa a 0 a 1
		float angulo = (Time.time + fase) / periodo * 2f * Mathf.PI;
		float onda = (Mathf.Sin(angulo) + 1f) * 0.5f;

		// alto.x es el alto mínimo y alto.y el máximo
		Vector3 escala = transform.localScale;
		escala.y = Mathf.Lerp(alto.x, alto.y, onda);
		transform.localScale = escala;

		if (jugador == null)
		{
			return;
		}

		// Los choques de este paso de física ya se procesaron: si tocó la parte roja, no hay impulso
		if (cuerpo.EstaRota || jugador.EstaMuerto)
		{
			Soltar();
			return;
		}

		if (Controles.InteractuarPresionado)
		{
			float velocidadX = jugador.VelocidadCarrera * multiplicadorVelocidad;
			jugador.Lanzar(altura, velocidadX);
			isUsado = true;
			Soltar();
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (isUsado || cuerpo.EstaRota)
		{
			return;
		}

		Player_controller entrante = other.GetComponentInParent<Player_controller>();
		if (entrante != null)
		{
			jugador = entrante; // Si en este mismo paso chocó con la parte roja, Update lo suelta
		}
	}

	// Sale del agua (o el chorro se achica y deja de tocarlo): ya no hay impulso
	void OnTriggerExit2D(Collider2D other)
	{
		if (jugador != null && other.GetComponentInParent<Player_controller>() == jugador)
		{
			Soltar();
		}
	}

	private void Soltar()
	{
		jugador = null;
	}
}
