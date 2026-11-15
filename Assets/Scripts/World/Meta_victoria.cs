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
// Desc: Al tocar la meta de cualquier carril termina la carrera y muestra la llegada a la oficina, antes de pasar al jefe.

using TMPro;
using UnityEngine;

// Las metas verdes (una al final de cada carril) están puestas en la escena; al tocar cualquiera se guarda el resultado
// (Resultado_carrera), se congela el juego y se muestra la pantalla de llegada. Su botón Continuar carga Level2 (Cambio_escena)
public class Meta_victoria : MonoBehaviour
{
	[SerializeField] private Caja_victoria[] paredes;
	[SerializeField] private Cronometro cronometro;
	[SerializeField] private Contador_recolectables contador;
	[SerializeField] private GameObject pantallaLlegada; // Empieza apagada
	[SerializeField] private TMP_Text textoTiempo;
	[SerializeField] private TMP_Text textoExcusas;

	private bool isTerminada;

	void Awake()
	{
		if (paredes == null || paredes.Length == 0 || System.Array.IndexOf(paredes, null) >= 0 || cronometro == null || contador == null ||
			pantallaLlegada == null || textoTiempo == null || textoExcusas == null)
		{
			Debug.LogError("Meta_victoria: faltan las paredes, cronometro, contador, pantallaLlegada o algún texto.");
			enabled = false;
			return;
		}

		foreach (Caja_victoria pared in paredes) pared.Iniciar(Llegar);
	}

	private void Llegar()
	{
		if (isTerminada) return; // Otro collider del jugador tocó la meta en el mismo cuadro
		isTerminada = true;

		cronometro.Detener();
		Resultado_carrera.Guardar(cronometro.Segundos, contador.Cantidad);
		textoTiempo.text = "Tiempo: " + Cronometro.Formatear(cronometro.Segundos);
		textoExcusas.text = "Excusas: " + contador.Cantidad;
		pantallaLlegada.SetActive(true);
		Estado_juego.Congelar();
	}
}
