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
// Desc: Elige la pista de la música de FMOD de la escena; la música sigue sonando entre escenas.

using FMOD.Studio;
using FMODUnity;
using UnityEngine;

// Uno por escena. Hay una sola instancia del evento para todo el juego: al cambiar de escena solo cambia el
// parámetro Pista y FMOD hace la transición (y reintentar el nivel no corta la música)
public class Musica_fmod : MonoBehaviour
{
	private const string Parametro = "Pista";

	[SerializeField] private EventReference eventoMusica;
	[SerializeField] private string pista; // Etiqueta del parámetro Pista en FMOD: Menu, Nivel o Jefe

	private static EventInstance instancia;

	// Fast Enter Play Mode no limpia los estáticos: sin esto, la segunda vez en el Editor usaría una instancia de un FMOD ya cerrado
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Reiniciar()
	{
		instancia = default;
	}

	void Awake()
	{
		if (eventoMusica.IsNull || string.IsNullOrEmpty(pista))
		{
			Debug.LogError("Musica_fmod: falta asignar eventoMusica o pista.");
			enabled = false;
			return;
		}
		if (!RuntimeManager.HaveAllBanksLoaded)
		{
			Debug.LogWarning("Musica_fmod: los bancos de FMOD no están cargados (FMOD → Refresh Banks).");
			enabled = false;
		}
	}

	void OnEnable()
	{
		Volumen_audio.MusicaCambiada += AplicarVolumen;
	}

	void OnDisable()
	{
		Volumen_audio.MusicaCambiada -= AplicarVolumen;
	}

	void Start()
	{
		bool isNueva = !instancia.isValid();
		if (isNueva) instancia = RuntimeManager.CreateInstance(eventoMusica);

		// Antes de start: si la escena no es el menú, arranca directo en su pista
		if (instancia.setParameterByNameWithLabel(Parametro, pista) != FMOD.RESULT.OK)
			Debug.LogWarning($"Musica_fmod: el evento no tiene el parámetro {Parametro} con la etiqueta «{pista}».");

		AplicarVolumen();
		if (isNueva) instancia.start();
	}

	private void AplicarVolumen()
	{
		instancia.setVolume(Volumen_audio.Musica); // FMOD no pasa por AudioSource: aplica el volumen elegido acá
	}
}
