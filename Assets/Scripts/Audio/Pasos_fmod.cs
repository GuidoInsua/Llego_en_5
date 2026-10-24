using FMOD.Studio;
using FMODUnity;
using UnityEngine;

// Va en el mismo objeto que el Animator (Modelo): los eventos del clip de correr llaman a Paso().
public class Pasos_fmod : MonoBehaviour
{
	[SerializeField] private EventReference eventoPaso;

	void Awake()
	{
		if (eventoPaso.IsNull)
		{
			Debug.LogError("Pasos_fmod: falta asignar eventoPaso.");
			enabled = false;
		}
	}

	// Lo llama el Animation Event del clip. Sin bancos cargados no suena (y no tira error en cada pisada)
	public void Paso()
	{
		if (!enabled || !RuntimeManager.HaveAllBanksLoaded) return;

		EventInstance instancia = RuntimeManager.CreateInstance(eventoPaso);
		instancia.setVolume(Volumen_audio.Efectos); // FMOD no pasa por AudioSource: aplica el volumen elegido acá
		instancia.start();
		instancia.release();
	}
}
