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

using System;
using UnityEngine;

// Caja verde de prueba: al tocarla el jugador avisa a quien la creó (Meta_victoria)
[RequireComponent(typeof(Collider2D))]
public class Caja_victoria : MonoBehaviour
{
	private Action alTocar;

	public void Iniciar(Action alTocar)
	{
		this.alTocar = alTocar;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponentInParent<Player_controller>() != null)
		{
			alTocar?.Invoke();
		}
	}
}
