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
// Desc: Tapa de alcantarilla: con interactuar encima, el jugador sale por el caño de abajo.

using UnityEngine;

// Va en el prefab CalleDobleConTunel, en un trigger sobre la tapa (capa Default: en Ground el sensor de suelo lo tomaría por suelo).
// La tecla se ve siempre en el prefab (hijo TeclaE). Mientras el jugador está encima, si aprieta interactuar (E o Espacio) entra y sale por el caño
public class Alcantarilla : MonoBehaviour
{
	[Tooltip("Dónde aparece el jugador: dentro del caño de salida, que se dibuja delante de él.")]
	[SerializeField] private Transform puntoSalida;
	[Tooltip("Velocidad hacia abajo con la que sale: sin ella, al avanzar mientras cae se asoma por el costado del caño.")]
	[SerializeField] private float velocidadSalida = 8f;
	[Tooltip("Partículas en la boca del caño (la cámara salta a la salida, así que en la tapa no se verían).")]
	[SerializeField] private ParticleSystem particulasSalida;

	private Player_controller jugador; // El que está encima de la tapa (null si no hay nadie)

	void Awake()
	{
		if (puntoSalida == null || particulasSalida == null)
		{
			Debug.LogError("Alcantarilla: falta asignar el punto de salida o las partículas.");
			enabled = false;
		}
	}

	void Update()
	{
		if (jugador == null)
		{
			return;
		}

		if (jugador.EstaMuerto)
		{
			Soltar();
			return;
		}

		if (Controles.InteractuarPresionado)
		{
			particulasSalida.Play();
			jugador.Teletransportar(puntoSalida.position, velocidadSalida);
			Soltar();
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		// Los triggers avisan aunque el script esté apagado
		if (!enabled)
		{
			return;
		}

		Player_controller entrante = other.GetComponentInParent<Player_controller>();
		if (entrante != null)
		{
			jugador = entrante;
		}
	}

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

	// Línea hasta el punto de salida, solo en el Editor con el objeto seleccionado
	void OnDrawGizmosSelected()
	{
		if (puntoSalida == null)
		{
			return;
		}

		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(transform.position, puntoSalida.position);
		Gizmos.DrawWireSphere(puntoSalida.position, 0.3f);
	}
}
