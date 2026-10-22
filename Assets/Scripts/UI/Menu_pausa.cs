using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Pausa con Escape: congela el juego y muestra continuar, menú principal y salir
public class Menu_pausa : MonoBehaviour
{
	[Tooltip("Empieza apagado; se prende al pausar.")]
	[SerializeField] private GameObject canvasPausa;
	[SerializeField] private string menuSceneName = "Menu"; // Tiene que estar agregada en Build Settings

	private bool isPausado;

	void Awake()
	{
		if (canvasPausa == null)
		{
			Debug.LogError("Menu_pausa: falta asignar el canvas de pausa.");
			enabled = false;
		}
	}

	void Update()
	{
		if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
		{
			if (isPausado) Continuar();
			else Pausar();
		}
	}

	// Al destruirse (cambio de escena o fin del Play) deja el tiempo y el audio como estaban
	void OnDestroy()
	{
		Reanudar();
	}

	private void Pausar()
	{
		isPausado = true;
		canvasPausa.SetActive(true);
		Time.timeScale = 0f;
		AudioListener.pause = true; // Corta también los pasos, que suenan en loop
	}

	// También la llama el botón Continuar
	public void Continuar()
	{
		isPausado = false;
		canvasPausa.SetActive(false);
		Reanudar();
	}

	public void IrAlMenu()
	{
		Reanudar();
		SceneManager.LoadScene(menuSceneName);
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
