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
// Desc: Teclas compartidas por varios scripts (interactuar con hidrantes y alcantarillas).

using UnityEngine;
using UnityEngine.InputSystem;

// Interactuar es E o la barra espaciadora. En pausa (timeScale 0) el input se ignora
public static class Controles
{
	public static bool InteractuarPresionado
	{
		get
		{
			Keyboard teclado = Keyboard.current;
			return Time.timeScale > 0f && teclado != null && (teclado.eKey.wasPressedThisFrame || teclado.spaceKey.wasPressedThisFrame);
		}
	}
}
