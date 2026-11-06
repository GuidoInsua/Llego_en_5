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
// Desc: Boca de un túnel: al deslizarse en ella se abre la tapa y el jugador baja deslizándose hasta el carril de abajo.

using UnityEngine;

// Prefab Tunel, para copiar y pegar en el mapa. El trigger de la raíz es la boca: si el jugador está deslizándose dentro, la tapa (solo ella)
// deja de ser sólida y el jugador baja con la física normal, chocando con las paredes del túnel, pero con la animación de slide fija hasta que
// se apoya en el suelo de abajo (`Salida`). Si pasa corriendo, la tapa lo deja seguir por arriba. La tapa se cierra al salir de la boca
// Las paredes y la tapa se arman a mano (Sprite Shape). `Salida` es un hijo vacío: se mueve sobre el suelo del carril de abajo; el jugador sale
// del túnel al apoyarse en un suelo que esté más bajo que ella (más `margenSalida`)
public class Tunel_suelo : MonoBehaviour
{
	[Tooltip("Collider de la tapa: lo único que el jugador atraviesa.")]
	[SerializeField] private Collider2D tapa;
	[Tooltip("Dónde termina el túnel: sobre el suelo del carril de abajo.")]
	[SerializeField] private Transform salida;
	[SerializeField] private float margenSalida = 2f;

	private Player_controller jugador;
	private bool isAbierto;

	void Awake()
	{
		if (tapa == null || salida == null)
		{
			Debug.LogError("Tunel_suelo: falta asignar la tapa o la salida.");
			enabled = false;
		}
	}

	void OnTriggerStay2D(Collider2D other)
	{
		if (isAbierto) return;

		if (jugador == null) jugador = other.GetComponentInParent<Player_controller>();
		if (jugador == null || !jugador.EstaDeslizando || jugador.EstaEnTunel || jugador.EstaMuerto) return;

		isAbierto = true;
		tapa.enabled = false;
		jugador.EntrarTunel(salida.position.y + margenSalida);
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (!isAbierto || other.GetComponentInParent<Player_controller>() == null) return;

		isAbierto = false;
		tapa.enabled = true;
	}

	void OnDrawGizmosSelected()
	{
		if (salida == null) return;

		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, salida.position);
		Gizmos.DrawWireSphere(salida.position, 0.3f);
	}
}
