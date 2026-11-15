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
// Desc: Reproduce un efecto de FMOD una vez, con el volumen de efectos de las opciones.

using FMOD.Studio;
using FMODUnity;

// El reproductor central de efectos: todo sonido suelto pasa por acá
public static class Sonido_fmod
{
	// Sin evento asignado o sin bancos cargados no suena (y no tira error en cada llamada)
	public static void Tocar(EventReference evento)
	{
		if (evento.IsNull || !RuntimeManager.HaveAllBanksLoaded) return;

		EventInstance instancia = RuntimeManager.CreateInstance(evento);
		instancia.setVolume(Volumen_audio.Efectos); // FMOD no pasa por AudioSource: aplica el volumen elegido acá
		instancia.start();
		instancia.release(); // Se libera sola al terminar de sonar
	}
}
