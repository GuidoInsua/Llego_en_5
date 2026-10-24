using UnityEngine;

// Aplica el volumen de música elegido en opciones al AudioSource de la música de la escena
[RequireComponent(typeof(AudioSource))]
public class Musica_volumen : MonoBehaviour
{
	void Awake()
	{
		GetComponent<AudioSource>().volume *= Volumen_audio.Musica;
	}
}
