using System.Collections.Generic;
using UnityEngine;

// Reproduce los efectos de sonido sin que el mismo clip se apile muchas veces a la vez.
// Va en un objeto de la escena con un AudioSource 2D. La música usa su propio AudioSource
[RequireComponent(typeof(AudioSource))]
public class Efectos_sonido : MonoBehaviour
{
	[Tooltip("Tiempo mínimo entre dos veces el mismo clip. Si varios golpes caen en el mismo frame suena uno solo, en vez de sumarse y saturar.")]
	[SerializeField] private float separacionMinima = 0.05f;

	private static Efectos_sonido instancia;
	private AudioSource fuente;
	private readonly Dictionary<AudioClip, float> ultimaVez = new Dictionary<AudioClip, float>();

	void Awake()
	{
		fuente = GetComponent<AudioSource>();
		instancia = this;
	}

	// Con Fast Enter Play Mode el estático sobrevive entre partidas: se limpia al destruir el objeto
	void OnDestroy()
	{
		if (instancia == this) instancia = null;
	}

	// Sin Efectos_sonido en la escena (o sin clip asignado) no suena nada
	public static void Reproducir(AudioClip clip)
	{
		if (clip == null || instancia == null) return;

		// unscaledTime: en pausa (timeScale 0) el tiempo normal no avanza
		float ahora = Time.unscaledTime;
		if (instancia.ultimaVez.TryGetValue(clip, out float ultima) && ahora - ultima < instancia.separacionMinima) return;

		instancia.ultimaVez[clip] = ahora;
		instancia.fuente.PlayOneShot(clip, Volumen_audio.Efectos);
	}
}
