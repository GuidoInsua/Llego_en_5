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
// Desc: Obstáculo que frena al jugador al chocar y titila; se queda en su lugar.

using System.Collections;
using UnityEngine;

// Obstáculo que se puede saltar. Al chocar con él titila con transparencia y se queda, sin collider: ya se lo chocó
[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class Obstaculo : MonoBehaviour
{
	[SerializeField] private float duracionRotura = 0.6f;

	private static readonly WaitForSeconds Parpadeo = new WaitForSeconds(0.1f); // Se reutiliza: evita crear uno nuevo por cada titilada

	private Collider2D col;
	private SpriteRenderer spriteRenderer;
	private bool isRota;

	void Awake()
	{
		col = GetComponent<Collider2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	// Verdadero desde el golpe: el hidrante lo usa para no ofrecer el impulso si ya chocaste con él
	public bool EstaRota
	{
		get { return isRota; }
	}

	public void Romper()
	{
		if (isRota)
		{
			return;
		}
		isRota = true;

		col.enabled = false; // Ya se lo chocó: deja pasar al jugador
		StartCoroutine(Titilar());
	}

	// Alterna entre transparente y normal cada 0,1 s, y al final lo deja con su color
	private IEnumerator Titilar()
	{
		Color original = spriteRenderer.color;
		float fin = Time.time + duracionRotura;
		bool isTransparente = false;

		while (Time.time < fin)
		{
			isTransparente = !isTransparente;

			float alfa;
			if (isTransparente)
			{
				alfa = 0.3f;
			}
			else
			{
				alfa = 1f;
			}
			spriteRenderer.color = new Color(original.r, original.g, original.b, alfa);

			yield return Parpadeo;
		}

		spriteRenderer.color = original;
	}
}
