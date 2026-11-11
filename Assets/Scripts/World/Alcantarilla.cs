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
// Desc: Tapa de alcantarilla: deslizarse justo encima lleva al jugador al caño de salida, en el subterráneo.

using UnityEngine;

// Va en el prefab CalleDobleConTunel, en un trigger sobre la tapa (capa Default: en Ground el sensor de suelo lo tomaría por suelo).
// Cuenta solo si el deslizamiento empieza con el jugador encima: si ya venía deslizándose al entrar, o lo aprieta después de pasar, no
public class Alcantarilla : MonoBehaviour
{
	[Tooltip("Dónde aparece el jugador: dentro del caño de salida, que se dibuja delante de él.")]
	[SerializeField] private Transform puntoSalida;
	[Tooltip("Velocidad hacia abajo con la que sale: sin ella, al avanzar mientras cae se asoma por el costado del caño.")]
	[SerializeField] private float velocidadSalida = 8f;
	[Tooltip("Partículas en la boca del caño (la cámara salta a la salida, así que en la tapa no se verían).")]
	[SerializeField] private ParticleSystem particulasSalida;

	private Player_controller jugador; // El que está encima de la tapa (null si no hay nadie)
	private bool isDeslizandoAntes;

	void Awake()
	{
		if (puntoSalida == null || particulasSalida == null)
		{
			Debug.LogError("Alcantarilla: falta asignar el punto de salida o las partículas.");
			enabled = false;
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		Player_controller entrante = other.GetComponentInParent<Player_controller>();
		if (entrante == null) return;

		jugador = entrante;
		isDeslizandoAntes = jugador.EstaDeslizando;
	}

	// El deslizamiento arranca en el FixedUpdate del jugador; esto corre después, en el mismo paso de física
	void OnTriggerStay2D(Collider2D other)
	{
		if (!enabled || jugador == null) return;

		bool isDeslizando = jugador.EstaDeslizando;
		if (isDeslizando && !isDeslizandoAntes && !jugador.EstaMuerto)
		{
			particulasSalida.Play();
			jugador.Teletransportar(puntoSalida.position, velocidadSalida);
			jugador = null;
			return;
		}
		isDeslizandoAntes = isDeslizando;
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (jugador != null && other.GetComponentInParent<Player_controller>() == jugador) jugador = null;
	}

	void OnDrawGizmosSelected()
	{
		if (puntoSalida == null) return;

		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(transform.position, puntoSalida.position);
		Gizmos.DrawWireSphere(puntoSalida.position, 0.3f);
	}
}
