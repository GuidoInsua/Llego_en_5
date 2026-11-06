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
// Desc: Muestra la pantalla de victoria al tocar la pared de la meta de cualquier carril.

using TMPro;
using UnityEngine;

// Las paredes verdes (una por carril) están puestas en la escena; al tocar cualquiera se muestra la pantalla de victoria con el tiempo
// Es para tener algo que probar: la meta real (el jefe) todavía no existe
public class Meta_victoria : MonoBehaviour
{
	[SerializeField] private Caja_victoria[] paredes;
	[SerializeField] private Cronometro cronometro;
	[SerializeField] private TMP_Text textoTiempoFinal;
	[SerializeField] private GameObject pantallaVictoria; // Empieza apagada

	void Awake()
	{
		if (paredes == null || paredes.Length == 0 || cronometro == null || textoTiempoFinal == null || pantallaVictoria == null)
		{
			Debug.LogError("Meta_victoria: faltan las paredes, cronometro, textoTiempoFinal o pantallaVictoria.");
			enabled = false;
			return;
		}

		foreach (Caja_victoria pared in paredes) pared.Iniciar(Ganar);
	}

	private void Ganar()
	{
		cronometro.Detener();
		textoTiempoFinal.text = "Tiempo: " + cronometro.Formato;
		pantallaVictoria.SetActive(true);
		Estado_juego.Congelar();
	}
}
