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

	// Static: se mantienen al cambiar de escena
	private static float tiempo;
	private static int excusas;

	public static float Tiempo
	{
		get { return tiempo; }
	}

	public static int Excusas
	{
		get { return excusas; }
	}

	// El mejor tiempo queda guardado en disco (PlayerPrefs), también al cerrar el juego
	public static float MejorTiempo
	{
		get { return PlayerPrefs.GetFloat(ClaveMejorTiempo); }
	}

	// Fast Enter Play Mode no limpia los estáticos: esto los vuelve a 0 en cada Play
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Limpiar()
	{
		tiempo = 0f;
		excusas = 0;
	}

	public static void Guardar(float tiempoCarrera, int excusasJuntadas)
	{
		tiempo = tiempoCarrera;
		excusas = excusasJuntadas;
	}

	// El récord cuenta solo si el jefe te deja volver al trabajo. Devuelve si lo es
	public static bool GuardarSiEsRecord()
	{
		// Es récord si todavía no había ninguno guardado, o si este tiempo es menor
		bool isRecord = !PlayerPrefs.HasKey(ClaveMejorTiempo) || tiempo < MejorTiempo;
		if (isRecord)
		{
			PlayerPrefs.SetFloat(ClaveMejorTiempo, tiempo);
			PlayerPrefs.Save();
		}
		return isRecord;
	}
}
