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
// Desc: Zona sobre un tramo: avisa al jugador lanzado desde un carril más bajo que ya llegó arriba.

using UnityEngine;

// Arma una zona (trigger) pegada a la parte de arriba del tramo, del ancho del collider. Va en el prefab Tramo; la zona va en un hijo en la capa Default:
// un trigger en la capa Ground lo tomaría por suelo el sensor del jugador
[RequireComponent(typeof(Collider2D), typeof(Carril_suelo))]
public class Zona_tramo : MonoBehaviour
{
	[Tooltip("Alto de la zona sobre la superficie del tramo.")]
	[SerializeField] private float alto = 6f;

	private int carril;

	void Awake()
	{
		carril = GetComponent<Carril_suelo>().Indice;

		Bounds limites = GetComponent<Collider2D>().bounds;
		GameObject zona = new GameObject("ZonaTramo");
		zona.transform.SetParent(transform, false);
		zona.transform.position = new Vector3(limites.center.x, limites.max.y + alto * 0.5f, 0f);

		BoxCollider2D caja = zona.AddComponent<BoxCollider2D>();
		caja.isTrigger = true;
		caja.size = new Vector2(limites.size.x, alto);

		zona.AddComponent<Aviso_tramo>().Iniciar(carril);
	}
}
