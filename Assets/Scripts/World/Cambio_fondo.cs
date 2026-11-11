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
// Desc: Cambia el fondo con parallax y el color del cielo según el carril que pisa el jugador.

using System.Collections;
using UnityEngine;

// Calle y edificios tienen cada uno su par de fondos (lejano y cercano) sobre el cielo original; el subterráneo, sin fondos y con otro cielo.
// Cambia al pisar el suelo del carril nuevo (CarrilCambiado), no en el aire
public class Cambio_fondo : MonoBehaviour
{
	[SerializeField] private Player_controller jugador;
	[SerializeField] private Camera camara;
	[Tooltip("El objeto que agrupa los fondos con parallax: se apaga en el subterráneo.")]
	[SerializeField] private GameObject fondos;
	[Tooltip("Cielo del subterráneo (sin fondos).")]
	[SerializeField] private Color colorAbajo = new Color(0.2f, 0.45f, 0.25f, 1f);
	[SerializeField] private float duracion = 1f; // Segundos que tarda el cielo en cambiar de color

	[Header("Fondos por carril")]
	[Tooltip("FondoLejano y sus copias.")]
	[SerializeField] private SpriteRenderer[] lejanos;
	[Tooltip("FondoCercano y sus copias.")]
	[SerializeField] private SpriteRenderer[] cercanos;
	[SerializeField] private Sprite lejanoCalle;
	[SerializeField] private Sprite cercanoCalle;
	[SerializeField] private Sprite lejanoEdificios;
	[SerializeField] private Sprite cercanoEdificios;

	private Color colorOriginal; // El de la cámara al empezar: el de la calle y los edificios
	private Coroutine cambio;

	void Awake()
	{
		if (jugador == null || camara == null || fondos == null || lejanos.Length == 0 || cercanos.Length == 0 ||
			lejanoCalle == null || cercanoCalle == null || lejanoEdificios == null || cercanoEdificios == null)
		{
			Debug.LogError("Cambio_fondo: falta asignar el jugador, la cámara, los fondos o los sprites de cada carril.");
			enabled = false;
			return;
		}

		colorOriginal = camara.backgroundColor;
		jugador.CarrilCambiado += Cambiar;
	}

	void OnDestroy()
	{
		if (jugador != null) jugador.CarrilCambiado -= Cambiar;
	}

	private void Cambiar(Carril carril)
	{
		if (carril == Carril.Subterraneo)
		{
			Aplicar(colorAbajo, false);
			return;
		}

		bool isCalle = carril == Carril.Calle;
		PonerSprite(lejanos, isCalle ? lejanoCalle : lejanoEdificios);
		PonerSprite(cercanos, isCalle ? cercanoCalle : cercanoEdificios);
		Aplicar(colorOriginal, true);
	}

	// Parallax_layer mide el ancho del sprite en cada cuadro: no hace falta que las imágenes midan lo mismo
	private static void PonerSprite(SpriteRenderer[] capas, Sprite sprite)
	{
		foreach (SpriteRenderer capa in capas) capa.sprite = sprite;
	}

	private void Aplicar(Color color, bool conFondos)
	{
		fondos.SetActive(conFondos);
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
