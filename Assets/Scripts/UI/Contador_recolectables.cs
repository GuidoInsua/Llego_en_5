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
// Desc: Contador en el HUD de recolectables juntados: la excusa vuela al ícono y el ícono pulsa.

using System.Collections;
using TMPro;
using UnityEngine;

// Cantidad de recolectables juntados, en el HUD
public class Contador_recolectables : MonoBehaviour
{
	[SerializeField] private TMP_Text texto;
	[SerializeField] private Player_controller jugador;

	[SerializeField] private RectTransform icono;

	[Header("Vuelo al HUD")]
	[SerializeField] private float duracionVuelo = 0.5f;
	[SerializeField] private float escalaFinalVuelo = 0.5f; // Respecto del tamaño al juntarla

	[Header("Pulso al llegar")]
	[SerializeField] private float duracionPulso = 0.25f;
	[SerializeField] private float escalaPulso = 1.4f;

	private int cantidad;
	private Coroutine pulso;
	private Camera camara;

	public int Cantidad
	{
		get { return cantidad; }
	}

	void Awake()
	{
		camara = Camera.main;
		if (texto == null || jugador == null || icono == null || camara == null)
		{
			Debug.LogError("Contador_recolectables: falta asignar el texto, el jugador o el ícono, o no hay cámara principal.");
			enabled = false;
			return;
		}

		jugador.Recolectado += Sumar;
		texto.text = "0";
	}

	void OnDestroy()
	{
		if (jugador != null)
		{
			jugador.Recolectado -= Sumar;
		}
	}

	// Cuenta al tocarla (si se gana en pleno vuelo, ya está sumada); el número cambia al llegar
	private void Sumar(Transform excusa)
	{
		cantidad++;
		StartCoroutine(Volar(excusa));
	}

	// Lleva la excusa (que está en el mundo) hasta donde se ve el ícono del HUD, acelerando, y ahí la destruye
	private IEnumerator Volar(Transform excusa)
	{
		Vector3 inicio = excusa.position;
		Vector3 escalaInicio = excusa.localScale;

		float tiempo = 0f;
		while (tiempo < duracionVuelo)
		{
			// El canvas está en Screen Space Overlay: la posición del ícono ya está en píxeles de la pantalla.
			// Se pasa a coordenadas del mundo en cada cuadro porque la cámara se mueve
			float distanciaACamara = inicio.z - camara.transform.position.z;
			Vector3 puntoEnPantalla = new Vector3(icono.position.x, icono.position.y, distanciaACamara);
			Vector3 destino = camara.ScreenToWorldPoint(puntoEnPantalla);

			// avance va de 0 a 1. Al cuadrado arranca lento y acelera
			float avance = tiempo / duracionVuelo;
			excusa.position = Vector3.Lerp(inicio, destino, avance * avance);

			float escala = Mathf.Lerp(1f, escalaFinalVuelo, avance);
			excusa.localScale = escalaInicio * escala;

			yield return null;
			tiempo += Time.deltaTime;
		}

		Destroy(excusa.gameObject);
		texto.text = cantidad.ToString();

		if (pulso != null)
		{
			StopCoroutine(pulso);
		}
		pulso = StartCoroutine(Pulso());
	}

	// Agranda el ícono y lo vuelve a su tamaño
	private IEnumerator Pulso()
	{
		float tiempo = 0f;
		while (tiempo < duracionPulso)
		{
			// El seno de 0 a PI va de 0 a 1 y vuelve a 0
			float avance = tiempo / duracionPulso;
			float curva = Mathf.Sin(avance * Mathf.PI);
			float escala = 1f + (escalaPulso - 1f) * curva;
			icono.localScale = Vector3.one * escala;

			yield return null;
			tiempo += Time.deltaTime;
		}

		icono.localScale = Vector3.one;
		pulso = null;
	}
}
