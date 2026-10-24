using UnityEngine;
using UnityEngine.SceneManagement;

// Botones que cambian de escena (reiniciar, volver al menú y salir). Tiene que estar agregada en Build Settings
public class Cambio_escena : MonoBehaviour
{
	// Desde la victoria el tiempo y el audio están congelados: la escena nueva arranca normal
	public void CargarEscena(string nombre)
	{
		Reanudar();
		SceneManager.LoadScene(nombre);
	}

	public void Reiniciar()
	{
		Reanudar();
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void Salir()
	{
		// En el Editor Application.Quit no cierra nada; solo funciona en el build
		Application.Quit();
	}

	private static void Reanudar()
	{
		Time.timeScale = 1f;
		AudioListener.pause = false;
	}
}
