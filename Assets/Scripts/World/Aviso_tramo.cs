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
// Desc: Trigger de Zona_tramo: avisa al jugador que entró en un tramo de ese carril.

using UnityEngine;

// Lo crea Zona_tramo en su hijo trigger
public class Aviso_tramo : MonoBehaviour
{
	private int carril;

	public void Iniciar(int carril)
	{
		this.carril = carril;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		Player_controller jugador = other.GetComponentInParent<Player_controller>();
		if (jugador != null) jugador.EntrarEnTramo(carril);
	}
}
