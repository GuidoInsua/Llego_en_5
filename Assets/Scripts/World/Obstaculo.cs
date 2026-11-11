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
// Desc: Caja que frena al jugador al chocar y desaparece.

using System.Collections;
using UnityEngine;

// Obstáculo que se puede saltar. Al chocar con él titila con transparencia y desaparece (o se queda, si es permanente)
[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class Obstaculo : MonoBehaviour
{
	[SerializeField] private float duracionRotura = 0.6f;
	[SerializeField] private Sprite[] variantes; // Mismo barril en distintos colores: cada uno toma uno al azar
	[Tooltip("Titila pero no desaparece (el hidrante). El collider queda apagado igual: ya se lo chocó.")]
	[SerializeField] private bool isPermanente;

	private static readonly WaitForSeconds Parpadeo = new WaitForSeconds(0.1f); // Se reutiliza: evita crear uno nuevo por cada titilada

	private Collider2D col;
	private SpriteRenderer spriteRenderer;
	private bool rota;

	void Awake()
	{
		col = GetComponent<Collider2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (variantes != null && variantes.Length > 0) spriteRenderer.sprite = variantes[Random.Range(0, variantes.Length)];
	}

	// Verdadero desde el golpe: el hidrante lo usa para no ofrecer el impulso si ya chocaste con él
	public bool EstaRota => rota;

	public void Romper()
	{
		if (rota) return;
		rota = true;

		col.enabled = false; // Dejamos pasar al jugador mientras la caja se rompe
		StartCoroutine(Titilar());
	}

	private IEnumerator Titilar()
	{
		Color original = spriteRenderer.color;
		float fin = Time.time + duracionRotura;
		bool transparente = false;

		while (Time.time < fin)
		{
			transparente = !transparente;
			float alpha = transparente ? 0.3f : 1f;
			spriteRenderer.color = new Color(original.r, original.g, original.b, alpha);
			yield return Parpadeo;
		}

		if (isPermanente) spriteRenderer.color = original;
		else Destroy(gameObject);
	}
}
