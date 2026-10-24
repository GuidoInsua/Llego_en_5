using UnityEngine;

// Aplica el volumen de música elegido en opciones al AudioSource de la música de la escena, también si cambia mientras suena
[RequireComponent(typeof(AudioSource))]
public class Musica_volumen : MonoBehaviour
{
	private AudioSource fuente;
	private float volumenBase; // El volumen del AudioSource en la escena, sin el de opciones

	void Awake()
	{
		fuente = GetComponent<AudioSource>();
		volumenBase = fuente.volume;
		Aplicar();
	}

	void OnEnable()
	{
		Volumen_audio.MusicaCambiada += Aplicar;
	}

	void OnDisable()
	{
		Volumen_audio.MusicaCambiada -= Aplicar;
	}

	private void Aplicar()
	{
		fuente.volume = volumenBase * Volumen_audio.Musica;
	}
}
