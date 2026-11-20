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
// Desc: Sonido ambiente del carril que pisa el jugador y eco en el subterráneo.

using FMOD.Studio;
using FMODUnity;
using UnityEngine;

// Un solo evento con el parámetro Carril (una etiqueta por carril, FMOD hace el fundido al cambiar).
// En el subterráneo prende el snapshot del eco, que en el mixer de FMOD le suma reverb a los efectos y al ambiente
public class Ambiente_fmod : MonoBehaviour
{
	private const string Parametro = "Carril";

	[SerializeField] private Player_controller jugador;
	[SerializeField] private EventReference eventoAmbiente;
	[SerializeField] private EventReference snapshotEco;

	private EventInstance ambiente;
	private EventInstance eco;
	private bool isSonando;

	void Awake()
	{
		if (jugador == null || eventoAmbiente.IsNull || snapshotEco.IsNull)
		{
			Debug.LogError("Ambiente_fmod: falta asignar el jugador, eventoAmbiente o snapshotEco.");
			enabled = false;
			return;
		}
		if (!RuntimeManager.HaveAllBanksLoaded)
		{
			Debug.LogWarning("Ambiente_fmod: los bancos de FMOD no están cargados (FMOD → Refresh Banks).");
			enabled = false;
			return;
		}

		ambiente = RuntimeManager.CreateInstance(eventoAmbiente);
		jugador.CarrilCambiado += Cambiar;
	}

	void OnEnable()
	{
		Volumen_audio.AmbienteCambiada += AplicarVolumen;
	}

	void OnDisable()
	{
		Volumen_audio.AmbienteCambiada -= AplicarVolumen;
	}

	// Al salir de la escena (menú, reintentar, jefe) se apagan los dos: si no, el eco seguiría en el menú
	void OnDestroy()
	{
		if (jugador != null)
		{
			jugador.CarrilCambiado -= Cambiar;
		}
		ApagarEco();
		ambiente.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		ambiente.release();
	}

	// La primera llamada llega al tocar el suelo por primera vez: recién ahí arranca, ya con el carril correcto
	private void Cambiar(Carril carril)
	{
		// Las etiquetas en FMOD se llaman igual que el enum: Subterraneo, Calle y Edificios
		FMOD.RESULT resultado = ambiente.setParameterByNameWithLabel(Parametro, carril.ToString());
		if (resultado != FMOD.RESULT.OK)
		{
			Debug.LogWarning("Ambiente_fmod: el evento no tiene el parámetro " + Parametro + " con la etiqueta «" + carril + "».");
		}

		if (!isSonando)
		{
			AplicarVolumen();
			ambiente.start();
			isSonando = true;
		}

		if (carril == Carril.Subterraneo)
		{
			PrenderEco();
		}
		else
		{
			ApagarEco();
		}
	}

	private void PrenderEco()
	{
		if (!eco.isValid())
		{
			eco = RuntimeManager.CreateInstance(snapshotEco);
			eco.start();
		}
	}

	private void ApagarEco()
	{
		if (eco.isValid())
		{
			eco.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			eco.release();
			eco = new EventInstance(); // Vacía: isValid() da falso y PrenderEco crea otra
		}
	}

	private void AplicarVolumen()
	{
		ambiente.setVolume(Volumen_audio.Ambiente); // FMOD no pasa por AudioSource: aplica el volumen elegido acá
	}
}
