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
// Desc: Sacude la cámara al chocar un obstáculo y al aterrizar de una caída larga.

using Unity.Cinemachine;
using UnityEngine;

// Va con un CinemachineImpulseSource; la CinemachineCamera necesita un CinemachineImpulseListener para recibir la sacudida
[RequireComponent(typeof(CinemachineImpulseSource))]
public class Sacudida_camara : MonoBehaviour
{
	[SerializeField] private Player_controller jugador;
	[Tooltip("Cuánto se corre la cámara al chocar un obstáculo (unidades del mundo).")]
	[SerializeField] private float fuerzaGolpe = 0.25f;
	[Tooltip("Caídas más lentas que esto no sacuden (un salto normal llega a ~11; bajar de los edificios a la calle, a ~40).")]
	[SerializeField] private float velocidadCaidaMinima = 20f;
	[SerializeField] private float fuerzaAterrizaje = 0.3f;

	private CinemachineImpulseSource fuente;

	void Awake()
	{
		fuente = GetComponent<CinemachineImpulseSource>();
		if (jugador == null)
		{
			Debug.LogError("Sacudida_camara: falta asignar el jugador.");
			enabled = false;
			return;
		}

		jugador.Golpeado += AlGolpear;
		jugador.Aterrizado += AlAterrizar;
	}

	void OnDestroy()
	{
		if (jugador == null) return;
		jugador.Golpeado -= AlGolpear;
		jugador.Aterrizado -= AlAterrizar;
	}

	// En diagonal hacia atrás: se lee como un tropezón contra lo que se chocó
	private void AlGolpear()
	{
		fuente.GenerateImpulse(new Vector3(-1f, -0.5f, 0f).normalized * fuerzaGolpe);
	}

	private void AlAterrizar(float velocidadCaida)
	{
		if (velocidadCaida >= velocidadCaidaMinima) fuente.GenerateImpulse(Vector3.down * fuerzaAterrizaje);
	}
}
