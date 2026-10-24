using UnityEngine;

// Punto que sigue la cámara: va con el jugador en horizontal, pero en vertical no rebota con cada salto.
// Se queda a la altura del último suelo, baja pegado al jugador si cae más de margenAbajo y se desliza al nuevo suelo al aterrizar
public class Camara_objetivo : MonoBehaviour
{
	[SerializeField] private Player_controller jugador;
	[Tooltip("Cuánto puede caer el jugador por debajo del punto antes de que este lo siga.")]
	[SerializeField] private float margenAbajo = 1f;
	[Tooltip("Segundos que tarda en acomodarse a la altura del suelo nuevo.")]
	[SerializeField] private float tiempoSuavizado = 0.3f;

	private float alturaActual;
	private float velocidadAltura;

	void Awake()
	{
		if (jugador == null)
		{
			Debug.LogError("Camara_objetivo: falta asignar el jugador.");
			enabled = false;
		}
	}

	void Start()
	{
		alturaActual = jugador.transform.position.y;
	}

	// LateUpdate a orden 0: corre antes que el CinemachineBrain (orden 200) y la cámara lee la posición ya actualizada
	void LateUpdate()
	{
		float alturaJugador = jugador.transform.position.y;

		if (alturaJugador < alturaActual - margenAbajo)
		{
			// Cayendo: el punto baja pegado al jugador para no perderlo
			alturaActual = alturaJugador + margenAbajo;
			velocidadAltura = 0f;
		}
		else if (jugador.EstaEnSuelo)
		{
			alturaActual = Mathf.SmoothDamp(alturaActual, alturaJugador, ref velocidadAltura, tiempoSuavizado);
		}

		transform.position = new Vector3(jugador.transform.position.x, alturaActual, jugador.transform.position.z);
	}
}
