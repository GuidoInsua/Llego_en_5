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
// Desc: Pantalla de controles: al entrar al nivel (salvo al reiniciar) congela el juego hasta apretar Empezar; también se abre desde Opciones.

using TMPro;
using UnityEngine;

// Va en CanvasInicio (Level1, prendido: se ve al arrancar; apagarlo en el Editor arranca a jugar directo) y en su copia de Menu (apagada).
// El botón del canvas llama a Empezar; el botón Controles de cada panel de opciones llama a Abrir pasándole ese panel
public class Pantalla_inicio : MonoBehaviour
{
	[Tooltip("Si se muestra al cargar la escena congelando el juego (Level1). En Menu va apagado: solo se abre desde Opciones.")]
	[SerializeField] private bool isAlArrancar = true;
	[Tooltip("Texto del botón: dice Volver si se abrió desde Opciones.")]
	[SerializeField] private TMP_Text textoBoton;
	[SerializeField] private string textoVolver = "Volver";

	private static bool isReinicio; // Al reiniciar (R o Reintentar) no se vuelven a mostrar los controles
	private GameObject panelOrigen; // Panel de opciones desde el que se abrió (null: pantalla de inicio)
	private string textoEmpezar;

	// Fast Enter Play Mode no recarga el dominio: el estático arranca limpio en cada Play
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Limpiar()
	{
		isReinicio = false;
	}

	public static void SaltearProxima()
	{
		isReinicio = true;
	}

	void Awake()
	{
		if (textoBoton == null)
		{
			Debug.LogError("Pantalla_inicio: falta asignar el texto del botón.");
			enabled = false;
			return;
		}

		textoEmpezar = textoBoton.text;
		if (!isAlArrancar) return;

		if (isReinicio)
		{
			isReinicio = false;
			gameObject.SetActive(false);
			return;
		}

		Estado_juego.Congelar();
	}

	public void Abrir(GameObject opciones)
	{
		panelOrigen = opciones;
		opciones.SetActive(false);
		gameObject.SetActive(true); // Primero se prende: si es la primera vez, Awake guarda el texto original
		textoBoton.text = textoVolver;
	}

	public void Empezar()
	{
		gameObject.SetActive(false);
		if (panelOrigen == null)
		{
			Estado_juego.Reanudar();
			return;
		}

		panelOrigen.SetActive(true);
		panelOrigen = null;
		textoBoton.text = textoEmpezar;
	}
}
