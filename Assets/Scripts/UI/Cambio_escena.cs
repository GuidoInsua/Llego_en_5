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
// Desc: Botones que reinician, vuelven al menú o salen del juego.

using UnityEngine;
using UnityEngine.SceneManagement;

// Botones que cambian de escena (reiniciar, volver al menú y salir). Tiene que estar agregada en Build Settings
public class Cambio_escena : MonoBehaviour
{
	// Desde la victoria el tiempo y el audio están congelados: la escena nueva arranca normal
	public void CargarEscena(string nombre)
	{
		Estado_juego.Reanudar();
		SceneManager.LoadScene(nombre);
	}

	public void Reiniciar()
	{
		Estado_juego.Reanudar();
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void Salir()
	{
		// En el Editor Application.Quit no cierra nada; solo funciona en el build
		Application.Quit();
	}

}
