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
// Desc: Mata al jugador si cae por debajo del carril de más abajo (se cayó por un hueco).

using UnityEngine;

// Trigger enorme bajo el subterráneo: en la calle y los edificios un hueco lleva al carril de abajo, acá no hay nada que frene la caída
[RequireComponent(typeof(Collider2D))]
public class Zona_muerte : MonoBehaviour
{
	void OnTriggerEnter2D(Collider2D other)
	{
		Player_controller jugador = other.GetComponentInParent<Player_controller>();
		if (jugador != null) jugador.Matar();
	}
}
