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
// Desc: Muestra la pantalla de victoria al tocar la meta de cualquier carril, con el tiempo, las excusas y el mejor tiempo.

using TMPro;
using UnityEngine;

// Las metas verdes (una al final de cada carril) están puestas en la escena; al tocar cualquiera se muestra la pantalla de victoria
// con el tiempo, las excusas juntadas y el mejor tiempo (guardado en PlayerPrefs)
public class Meta_victoria : MonoBehaviour
{
	private const string ClaveMejorTiempo = "MejorTiempo";

	[SerializeField] private Caja_victoria[] paredes;
	[SerializeField] private Cronometro cronometro;
	[SerializeField] private Contador_recolectables contador;
	[SerializeField] private TMP_Text textoTiempoFinal;
	[SerializeField] private TMP_Text textoMejorTiempo;
	[SerializeField] private TMP_Text textoExcusas;
	[SerializeField] private GameObject pantallaVictoria; // Empieza apagada

	void Awake()
	{
		if (paredes == null || paredes.Length == 0 || System.Array.IndexOf(paredes, null) >= 0 || cronometro == null || contador == null ||
			textoTiempoFinal == null || textoMejorTiempo == null || textoExcusas == null || pantallaVictoria == null)
		{
			Debug.LogError("Meta_victoria: faltan las paredes, cronometro, contador, algún texto o pantallaVictoria.");
			enabled = false;
			return;
		}

		foreach (Caja_victoria pared in paredes) pared.Iniciar(Ganar);
	}

	private void Ganar()
	{
		cronometro.Detener();
		float tiempo = cronometro.Segundos;

		bool isRecord = !PlayerPrefs.HasKey(ClaveMejorTiempo) || tiempo < PlayerPrefs.GetFloat(ClaveMejorTiempo);
		if (isRecord)
		{
			PlayerPrefs.SetFloat(ClaveMejorTiempo, tiempo);
			PlayerPrefs.Save();
		}

		textoTiempoFinal.text = "Tiempo: " + Cronometro.Formatear(tiempo);
		textoMejorTiempo.text = "Mejor tiempo: " + Cronometro.Formatear(PlayerPrefs.GetFloat(ClaveMejorTiempo)) + (isRecord ? " (¡nuevo récord!)" : "");
		textoExcusas.text = "Excusas: " + contador.Cantidad;
		pantallaVictoria.SetActive(true);
		Estado_juego.Congelar();
	}
}
