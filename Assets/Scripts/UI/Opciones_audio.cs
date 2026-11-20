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
// Desc: Panel de opciones con sliders de música y efectos.

using UnityEngine;
using UnityEngine.UI;

// Panel de opciones: un slider para la música, otro para los efectos y otro para el sonido ambiente
public class Opciones_audio : MonoBehaviour
{
	[SerializeField] private Slider sliderMusica;
	[SerializeField] private Slider sliderEfectos;
	[SerializeField] private Slider sliderAmbiente;

	void Awake()
	{
		if (sliderMusica == null || sliderEfectos == null || sliderAmbiente == null)
		{
			Debug.LogError("Opciones_audio: falta asignar los sliders.");
			enabled = false;
			return;
		}

		sliderMusica.onValueChanged.AddListener(CambiarVolumenMusica);
		sliderEfectos.onValueChanged.AddListener(CambiarVolumenEfectos);
		sliderAmbiente.onValueChanged.AddListener(CambiarVolumenAmbiente);
	}

	// Cada vez que se abre muestra lo guardado
	void OnEnable()
	{
		if (!enabled)
		{
			return;
		}

		sliderMusica.SetValueWithoutNotify(Volumen_audio.Musica);
		sliderEfectos.SetValueWithoutNotify(Volumen_audio.Efectos);
		sliderAmbiente.SetValueWithoutNotify(Volumen_audio.Ambiente);
	}

	// Al cerrar el panel se guarda a disco: si el juego se cierra mal, no se pierde el volumen
	void OnDisable()
	{
		PlayerPrefs.Save();
	}

	private void CambiarVolumenMusica(float valor)
	{
		Volumen_audio.Musica = valor;
	}

	private void CambiarVolumenEfectos(float valor)
	{
		Volumen_audio.Efectos = valor;
	}

	private void CambiarVolumenAmbiente(float valor)
	{
		Volumen_audio.Ambiente = valor;
	}
}
