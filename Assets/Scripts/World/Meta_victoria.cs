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
		bool isFaltaAlgo = cronometro == null || contador == null || pantallaLlegada == null || textoTiempo == null || textoExcusas == null;

		if (paredes == null || paredes.Length == 0)
		{
			isFaltaAlgo = true;
		}
		else
		{
			foreach (Caja_victoria pared in paredes)
			{
				if (pared == null)
				{
					isFaltaAlgo = true;
				}
			}
		}

		if (isFaltaAlgo)
		{
			Debug.LogError("Meta_victoria: faltan las paredes, cronometro, contador, pantallaLlegada o algún texto.");
			enabled = false;
			return;
		}

		// Cada meta se guarda esta Meta_victoria para avisarle cuando el jugador la toque
		foreach (Caja_victoria pared in paredes)
		{
			pared.Iniciar(this);
		}
	}

	// Lo llama Caja_victoria al tocarla el jugador
	public void Llegar()
	{
		// Otro collider del jugador ya tocó la meta en el mismo cuadro
		if (isTerminada)
		{
			return;
		}
		isTerminada = true;

		cronometro.Detener();
		Resultado_carrera.Guardar(cronometro.Segundos, contador.Cantidad);
		textoTiempo.text = "Tiempo: " + Cronometro.Formatear(cronometro.Segundos);
		textoExcusas.text = "Excusas: " + contador.Cantidad;
		pantallaLlegada.SetActive(true);
		Estado_juego.Congelar();
	}
}
