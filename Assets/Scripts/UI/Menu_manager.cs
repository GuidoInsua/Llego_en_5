using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu_manager : MonoBehaviour
{
	[Header("Botones")]
	[SerializeField] private Button startButton;
	[SerializeField] private Button quitButton;

	[Header("Escenas")]
	[SerializeField] private string gameSceneName = "Level1"; // Tiene que estar agregada en Build Settings

	void Awake()
	{
		if (startButton == null || quitButton == null)
		{
			Debug.LogError("Menu_manager: falta asignar startButton o quitButton.");
			enabled = false;
			return;
		}

		startButton.onClick.AddListener(IniciarJuego);
		quitButton.onClick.AddListener(SalirDelJuego);
	}

	void OnDestroy()
	{
		if (startButton != null) startButton.onClick.RemoveListener(IniciarJuego);
		if (quitButton != null) quitButton.onClick.RemoveListener(SalirDelJuego);
	}

	private void IniciarJuego()
	{
		SceneManager.LoadScene(gameSceneName);
	}

	private void SalirDelJuego()
	{
		// En el Editor Application.Quit no cierra nada; solo funciona en el build
		Application.Quit();
	}
}
