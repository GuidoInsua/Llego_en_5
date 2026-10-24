using UnityEngine;
using UnityEngine.UI;

// Panel de opciones: un slider para la música y otro para los efectos
public class Opciones_audio : MonoBehaviour
{
	[SerializeField] private Slider sliderMusica;
	[SerializeField] private Slider sliderEfectos;

	void Awake()
	{
		if (sliderMusica == null || sliderEfectos == null)
		{
			Debug.LogError("Opciones_audio: falta asignar los sliders.");
			enabled = false;
			return;
		}

		sliderMusica.onValueChanged.AddListener(CambiarVolumenMusica);
		sliderEfectos.onValueChanged.AddListener(CambiarVolumenEfectos);
	}

	// Cada vez que se abre muestra lo guardado
	void OnEnable()
	{
		if (!enabled) return;

		sliderMusica.SetValueWithoutNotify(Volumen_audio.Musica);
		sliderEfectos.SetValueWithoutNotify(Volumen_audio.Efectos);
	}

	private void CambiarVolumenMusica(float valor)
	{
		Volumen_audio.Musica = valor;
	}

	private void CambiarVolumenEfectos(float valor)
	{
		Volumen_audio.Efectos = valor;
	}
}
