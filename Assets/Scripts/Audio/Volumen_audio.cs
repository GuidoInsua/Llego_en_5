using UnityEngine;

// Volúmenes elegidos en opciones (0 a 1), guardados en PlayerPrefs. Todo sonido nuevo tiene que respetarlos
public static class Volumen_audio
{
	private const string ClaveMusica = "VolumenMusica";
	private const string ClaveEfectos = "VolumenEfectos";

	// Quien se suscribe se baja en OnDisable (Fast Enter Play Mode no limpia los estáticos)
	public static event System.Action MusicaCambiada;

	public static float Musica
	{
		get { return PlayerPrefs.GetFloat(ClaveMusica, 1f); }
		set
		{
			PlayerPrefs.SetFloat(ClaveMusica, Mathf.Clamp01(value));
			MusicaCambiada?.Invoke();
		}
	}

	public static float Efectos
	{
		get { return PlayerPrefs.GetFloat(ClaveEfectos, 1f); }
		set { PlayerPrefs.SetFloat(ClaveEfectos, Mathf.Clamp01(value)); }
	}
}
