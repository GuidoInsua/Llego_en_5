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
// Desc: Pausa con Escape (continuar, opciones, menú y salir) y R para reintentar desde la pausa, la victoria o el game over.

using UnityEngine;
using UnityEngine.InputSystem;

// Pausa con Escape: congela el juego y muestra los botones (que cambian de escena con Cambio_escena)
public class Menu_pausa : MonoBehaviour
{
	[Tooltip("Empieza apagado; se prende al pausar.")]
	[SerializeField] private GameObject canvasPausa;
	[Tooltip("Los botones de la pausa y el panel de opciones: al pausar se ven los botones.")]
	[SerializeField] private GameObject botones;
	[SerializeField] private GameObject panelOpciones;
	[Tooltip("Si está a la vista (ganaste), Escape no pausa.")]
	[SerializeField] private GameObject pantallaVictoria;
	[Tooltip("Si está a la vista (perdiste), Escape tampoco pausa.")]
	[SerializeField] private GameObject pantallaGameOver;
	[Tooltip("Si está a la vista (controles al arrancar o abiertos desde Opciones), Escape no hace nada.")]
	[SerializeField] private GameObject pantallaInicio;
	[Tooltip("Reinicia el nivel con R en cualquier momento.")]
	[SerializeField] private Cambio_escena cambioEscena;

	private bool isPausado;

	void Awake()
	{
		if (canvasPausa == null || botones == null || panelOpciones == null || pantallaVictoria == null || pantallaGameOver == null || pantallaInicio == null || cambioEscena == null)
		{
			Debug.LogError("Menu_pausa: falta asignar el canvas de pausa, los botones, el panel de opciones, la pantalla de victoria, la de game over, la de inicio o Cambio_escena.");
			enabled = false;
		}
	}

	void Update()
	{
		if (Keyboard.current == null) return;

		bool isTerminado = pantallaVictoria.activeSelf || pantallaGameOver.activeSelf;
		// Con los controles a la vista (al arrancar o abiertos desde Opciones de la pausa) Escape no hace nada: se cierran con su botón
		if (Keyboard.current.escapeKey.wasPressedThisFrame && !pantallaInicio.activeSelf)
		{
			if (isPausado) Continuar();
			else if (!isTerminado) Pausar();
		}

		// Reintento inmediato, sin buscar el botón, también jugando
		if (Keyboard.current.rKey.wasPressedThisFrame) cambioEscena.Reiniciar();
	}

	// Al destruirse (cambio de escena o fin del Play) deja el tiempo y el audio como estaban
	void OnDestroy()
	{
		Estado_juego.Reanudar();
	}

	private void Pausar()
	{
		isPausado = true;
		canvasPausa.SetActive(true);
		botones.SetActive(true);
		panelOpciones.SetActive(false);
		Estado_juego.Congelar(); // Pausa la música y el ambiente de Unity (FMOD no depende de esto: sin tiempo no hay eventos de pasos)
	}

	// También la llama el botón Continuar
	public void Continuar()
	{
		isPausado = false;
		canvasPausa.SetActive(false);
		Estado_juego.Reanudar();
	}

}
