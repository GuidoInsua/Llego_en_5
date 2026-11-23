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
// Desc: Guarda los volúmenes de música y efectos elegidos en opciones.

using UnityEngine;

// Volúmenes elegidos en opciones (0 a 1), guardados en PlayerPrefs. Todo sonido nuevo tiene que respetarlos
public static class Volumen_audio
{
	private const string ClaveMusica = "VolumenMusica";
	private const string ClaveEfectos = "VolumenEfectos";
	private const string ClaveAmbiente = "VolumenAmbiente";

	// Quien se suscribe se baja en OnDisable (Fast Enter Play Mode no limpia los estáticos)
	public static event System.Action MusicaCambiada;
	public static event System.Action EfectosCambiada; // Solo la escuchan los efectos en loop (los sueltos leen el volumen al sonar)
	public static event System.Action AmbienteCambiada;

	public static float Musica
	{
		get { return PlayerPrefs.GetFloat(ClaveMusica, 1f); }
		set
		{
			// Clamp01 lo deja entre 0 y 1
			PlayerPrefs.SetFloat(ClaveMusica, Mathf.Clamp01(value));
			if (MusicaCambiada != null)
			{
				MusicaCambiada();
			}
		}
	}

	public static float Efectos
	{
		get { return PlayerPrefs.GetFloat(ClaveEfectos, 1f); }
		set
		{
			PlayerPrefs.SetFloat(ClaveEfectos, Mathf.Clamp01(value));
			if (EfectosCambiada != null)
			{
				EfectosCambiada();
			}
		}
	}

	// Lo de que suene más bajo que la música va en el mixer de FMOD (bus Ambiente), no acá
	public static float Ambiente
	{
		get { return PlayerPrefs.GetFloat(ClaveAmbiente, 1f); }
		set
		{
			PlayerPrefs.SetFloat(ClaveAmbiente, Mathf.Clamp01(value));
			if (AmbienteCambiada != null)
			{
				AmbienteCambiada();
			}
		}
	}
}
