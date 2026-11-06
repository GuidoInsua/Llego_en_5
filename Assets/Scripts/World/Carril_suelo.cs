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
// Desc: Marca de qué carril es un suelo (0 = el de más abajo), para que el jugador sepa si cayó de un carril o murió.

using UnityEngine;

// Va en cada tramo de suelo (el objeto que tiene el collider)
public class Carril_suelo : MonoBehaviour
{
	[SerializeField] private int indice;

	public int Indice => indice;

	public void Iniciar(int indice)
	{
		this.indice = indice;
	}
}
