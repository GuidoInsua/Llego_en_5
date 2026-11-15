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
// Desc: Tiempo y excusas de la carrera, que pasan del nivel a la escena del jefe, y el mejor tiempo guardado.

using UnityEngine;

// La meta lo guarda y el jefe lo lee. Si Level2 arranca directo en el Editor, queda en 0
public static class Resultado_carrera
{
	private const string ClaveMejorTiempo = "MejorTiempo";

	public static float Tiempo { get; private set; }
	public static int Excusas { get; private set; }
	public static float MejorTiempo => PlayerPrefs.GetFloat(ClaveMejorTiempo);

	// Fast Enter Play Mode no limpia los estáticos
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Limpiar()
	{
		Tiempo = 0f;
		Excusas = 0;
	}

	public static void Guardar(float tiempo, int excusas)
	{
		Tiempo = tiempo;
		Excusas = excusas;
	}

	// El récord cuenta solo si el jefe te deja volver al trabajo. Devuelve si lo es
	public static bool GuardarSiEsRecord()
	{
		bool isRecord = !PlayerPrefs.HasKey(ClaveMejorTiempo) || Tiempo < MejorTiempo;
		if (isRecord)
		{
			PlayerPrefs.SetFloat(ClaveMejorTiempo, Tiempo);
			PlayerPrefs.Save();
		}
		return isRecord;
	}
}
