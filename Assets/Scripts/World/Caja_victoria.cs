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
// Desc: Caja de la meta: avisa a Meta_victoria cuando el jugador la toca.

using UnityEngine;

// Meta (arco de llegada) al final de un carril: al tocarla el jugador avisa a Meta_victoria
[RequireComponent(typeof(Collider2D))]
public class Caja_victoria : MonoBehaviour
{
	private Meta_victoria meta; // La pone Meta_victoria al arrancar

	public void Iniciar(Meta_victoria meta)
	{
		this.meta = meta;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		bool isJugador = other.GetComponentInParent<Player_controller>() != null;
		if (isJugador && meta != null)
		{
			meta.Llegar();
		}
	}
}
