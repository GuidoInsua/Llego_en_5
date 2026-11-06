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
// Desc: Contador en el HUD de recolectables juntados.

using TMPro;
using UnityEngine;

// Cantidad de recolectables juntados, en el HUD
public class Contador_recolectables : MonoBehaviour
{
	[SerializeField] private TMP_Text texto;
	[SerializeField] private Player_controller jugador;

	[SerializeField] private string prefijo = "Excusas: ";

	private int cantidad;

	void Awake()
	{
		if (texto == null || jugador == null)
		{
			Debug.LogError("Contador_recolectables: falta asignar el texto o el jugador.");
			enabled = false;
			return;
		}

		jugador.Recolectado += Sumar;
		texto.text = prefijo + 0;
	}

	void OnDestroy()
	{
		if (jugador != null) jugador.Recolectado -= Sumar;
	}

	private void Sumar()
	{
		cantidad++;
		texto.text = prefijo + cantidad;
	}
}
