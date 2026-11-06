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
// Desc: Pieza amarilla que tapa el hueco entre dos tramos: se puede pisar (rampa) pero frena al jugador.

using UnityEngine;

// Va en el objeto del Sprite Shape amarillo que une dos tramos (prefab PuenteSuelo): el collider es sólido, de la capa del suelo
// El jugador puede subir y bajar por la rampa, pero más lento que saltando de tramo en tramo
[RequireComponent(typeof(Collider2D))]
public class Puente_suelo : MonoBehaviour
{
	private Player_controller jugador;

	void OnCollisionStay2D(Collision2D collision)
	{
		if (jugador == null) jugador = collision.gameObject.GetComponentInParent<Player_controller>();
		if (jugador != null) jugador.Ralentizar();
	}
}
