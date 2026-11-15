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
// Desc: Suena el evento de FMOD de los pasos desde los eventos de animación.

using FMODUnity;
using UnityEngine;

// Va en el mismo objeto que el Animator (Modelo): los eventos del clip de correr llaman a Paso().
public class Pasos_fmod : MonoBehaviour
{
	[SerializeField] private EventReference eventoPaso;

	void Awake()
	{
		if (eventoPaso.IsNull)
		{
			Debug.LogError("Pasos_fmod: falta asignar eventoPaso.");
			enabled = false;
		}
	}

	// Lo llama el Animation Event del clip
	public void Paso()
	{
		if (enabled) Sonido_fmod.Tocar(eventoPaso);
	}
}
