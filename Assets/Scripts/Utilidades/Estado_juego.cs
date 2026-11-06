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
// Desc: Congela y reanuda el juego (tiempo y audio de Unity) en un solo lugar.

using UnityEngine;

// Pausa, victoria y game over congelan el juego igual; cambiar de escena lo reanuda
public static class Estado_juego
{
	public static void Congelar()
	{
		Time.timeScale = 0f;
		AudioListener.pause = true;
	}

	public static void Reanudar()
	{
		Time.timeScale = 1f;
		AudioListener.pause = false;
	}
}
