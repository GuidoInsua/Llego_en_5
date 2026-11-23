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
// Desc: Efecto en loop pegado a un objeto, que suena más fuerte cuanto más cerca está el jugador.

using FMOD.Studio;
using FMODUnity;
using UnityEngine;

// El evento de FMOD tiene que ser 3D (Spatializer). El radio se ajusta acá (pisa el del evento) y se ve con el objeto seleccionado.
// La distancia se mide desde el jugador porque el StudioListener de la cámara tiene al jugador como attenuationObject
public class Sonido_posicional : MonoBehaviour
{
	[SerializeField] private EventReference evento;
	[Tooltip("Hasta esta distancia del jugador suena a todo volumen (círculo amarillo).")]
	[SerializeField] private float distanciaMinima = 2f;
	[Tooltip("Desde esta distancia ya no se escucha; en el medio va bajando (círculo celeste).")]
	[SerializeField] private float distanciaMaxima = 15f;

	private EventInstance instancia;

	void Start()
	{
		if (evento.IsNull)
		{
			Debug.LogError("Sonido_posicional: falta asignar el evento en " + name + ".");
			enabled = false;
			return;
		}
		if (!RuntimeManager.HaveAllBanksLoaded)
		{
			enabled = false; // Sin bancos no suena, como Sonido_fmod
			return;
		}

		instancia = RuntimeManager.CreateInstance(evento);
		RuntimeManager.AttachInstanceToGameObject(instancia, gameObject);
		instancia.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, distanciaMinima);
		instancia.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, distanciaMaxima);
		AplicarVolumen();
		instancia.start();
		Volumen_audio.EfectosCambiada += AplicarVolumen;
	}

	void OnDestroy()
	{
		Volumen_audio.EfectosCambiada -= AplicarVolumen;
		if (instancia.isValid())
		{
			instancia.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			instancia.release();
		}
	}

	private void AplicarVolumen()
	{
		instancia.setVolume(Volumen_audio.Efectos); // FMOD no pasa por AudioSource: aplica el volumen elegido acá
	}

#if UNITY_EDITOR
	// Los dos círculos del radio en la escena, al seleccionar el objeto
	void OnDrawGizmosSelected()
	{
		UnityEditor.Handles.color = Color.yellow;
		UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, distanciaMinima);
		UnityEditor.Handles.color = Color.cyan;
		UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, distanciaMaxima);
	}
#endif
}
