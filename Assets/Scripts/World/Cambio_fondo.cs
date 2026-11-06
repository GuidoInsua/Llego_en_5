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
// Desc: Al lanzarse el jugador hacia arriba, saca los edificios y oscurece el cielo.

using System.Collections;
using UnityEngine;

// Cuando el jugador sale disparado por un hidrante, el fondo de edificios (parallax) desaparece y el cielo pasa a un azul más oscuro
public class Cambio_fondo : MonoBehaviour
{
	[SerializeField] private Player_controller jugador;
	[SerializeField] private Camera camara;
	[Tooltip("El objeto que agrupa los fondos con parallax: se apaga.")]
	[SerializeField] private GameObject fondos;
	[Tooltip("Cielo del carril de arriba (sin edificios).")]
	[SerializeField] private Color colorCielo = new Color(0.22f, 0.32f, 0.5f, 1f);
	[Tooltip("Cielo del carril de abajo (sin edificios).")]
	[SerializeField] private Color colorAbajo = new Color(0.2f, 0.45f, 0.25f, 1f);
	[SerializeField] private float duracion = 1f; // Segundos que tarda el cielo en cambiar de color

	private Color colorMedio; // El de la cámara al empezar: carril del medio, con edificios
	private Coroutine cambio;

	void Awake()
	{
		if (jugador == null || camara == null || fondos == null)
		{
			Debug.LogError("Cambio_fondo: falta asignar el jugador, la cámara o los fondos.");
			enabled = false;
			return;
		}

		colorMedio = camara.backgroundColor;
		jugador.Lanzado += CambiarArriba;
		jugador.CarrilCambiado += Cambiar;
	}

	void OnDestroy()
	{
		if (jugador == null) return;
		jugador.Lanzado -= CambiarArriba;
		jugador.CarrilCambiado -= Cambiar;
	}

	// El lanzamiento cambia el cielo en pleno vuelo, sin esperar a aterrizar
	private void CambiarArriba()
	{
		Aplicar(colorCielo, false);
	}

	// 0 = abajo, el del medio es el original, el resto = arriba
	private void Cambiar(int carril)
	{
		if (carril == 0) Aplicar(colorAbajo, false);
		else if (carril == 1) Aplicar(colorMedio, true);
		else Aplicar(colorCielo, false);
	}

	private void Aplicar(Color color, bool conEdificios)
	{
		fondos.SetActive(conEdificios);
		if (cambio != null) StopCoroutine(cambio);
		cambio = StartCoroutine(CambiarCielo(color));
	}

	private IEnumerator CambiarCielo(Color colorCielo)
	{
		Color inicio = camara.backgroundColor;
		for (float t = 0f; t < duracion; t += Time.deltaTime)
		{
			camara.backgroundColor = Color.Lerp(inicio, colorCielo, t / duracion);
			yield return null;
		}

		camara.backgroundColor = colorCielo;
	}
}
