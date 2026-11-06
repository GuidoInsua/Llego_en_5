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
// Desc: Moneda blanca que el jugador agarra al tocarla; pulsa y tiene un brillo.

using UnityEngine;

// Trigger: al tocar al jugador le avisa y desaparece. Pulsa de tamaño y tiene un halo que late
[RequireComponent(typeof(Collider2D))]
public class Recolectable : MonoBehaviour
{
	[Header("Pulso")]
	[SerializeField] private float velocidadPulso = 3f;
	[SerializeField] private float amplitudPulso = 0.12f;

	[Header("Brillo")]
	[SerializeField] private float escalaBrillo = 2f;
	[SerializeField] private float alfaBrillo = 0.35f;

	private Vector3 escalaBase;
	private SpriteRenderer brillo;
	private float fase;

	void Awake()
	{
		escalaBase = transform.localScale;
		fase = Random.value * Mathf.PI * 2f; // así no pulsan todos a la vez

		// El halo es un hijo con el mismo sprite, más grande y translúcido
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		GameObject halo = new GameObject("Brillo");
		halo.transform.SetParent(transform, false);
		halo.transform.localScale = Vector3.one * escalaBrillo;
		brillo = halo.AddComponent<SpriteRenderer>();
		brillo.sprite = sr.sprite;
		brillo.sharedMaterial = sr.sharedMaterial;
		brillo.sortingLayerID = sr.sortingLayerID;
		brillo.sortingOrder = sr.sortingOrder - 1;
	}

	void Update()
	{
		float seno = Mathf.Sin(Time.time * velocidadPulso + fase);
		transform.localScale = escalaBase * (1f + amplitudPulso * seno);
		brillo.color = new Color(1f, 1f, 1f, alfaBrillo * (0.6f + 0.4f * seno));
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		Player_controller jugador = other.GetComponentInParent<Player_controller>();
		if (jugador == null) return;

		jugador.Recolectar();
		Destroy(gameObject);
	}
}
