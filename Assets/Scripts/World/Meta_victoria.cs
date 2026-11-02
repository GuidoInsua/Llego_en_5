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
// Desc: Pone la pared de la meta y muestra la pantalla de victoria.

using TMPro;
using UnityEngine;

// Cuando el suelo genera el tramo de la meta pone ahí una pared verde; al tocarla se muestra la pantalla de victoria con el tiempo
// Es para tener algo que probar: la meta real (el jefe) todavía no existe
public class Meta_victoria : MonoBehaviour
{
	[SerializeField] private Ground_spawner spawner;
	[SerializeField] private Caja_victoria cajaPrefab; // Pivote abajo: se apoya sobre el suelo
	[SerializeField] private float alturaPared = 20f; // Se estira hacia arriba para que no se pueda saltar por encima
	[SerializeField] private Cronometro cronometro;
	[SerializeField] private TMP_Text textoTiempoFinal;
	[SerializeField] private GameObject pantallaVictoria; // Empieza apagada

	void Awake()
	{
		if (spawner == null || cajaPrefab == null || cronometro == null || textoTiempoFinal == null || pantallaVictoria == null)
		{
			Debug.LogError("Meta_victoria: falta spawner, cajaPrefab, cronometro, textoTiempoFinal o pantallaVictoria.");
			enabled = false;
			return;
		}

		spawner.TramoMetaCreado += CrearPared;
	}

	void OnDestroy()
	{
		if (spawner != null) spawner.TramoMetaCreado -= CrearPared;
	}

	private void CrearPared(Vector2 posicion)
	{
		Caja_victoria pared = Instantiate(cajaPrefab, posicion, Quaternion.identity);
		pared.transform.localScale = new Vector3(1f, alturaPared, 1f);
		pared.Iniciar(Ganar);
	}

	private void Ganar()
	{
		cronometro.Detener();
		textoTiempoFinal.text = "Tiempo: " + cronometro.Formato;
		pantallaVictoria.SetActive(true);
		Time.timeScale = 0f;
		AudioListener.pause = true;
	}
}
