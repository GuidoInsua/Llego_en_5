using System.Collections;
using UnityEngine;

// Caja amarilla que se puede saltar. Al chocar con ella titila con transparencia y desaparece
[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class Obstaculo : MonoBehaviour
{
	[SerializeField] private float duracionRotura = 0.6f;

	private Collider2D col;
	private SpriteRenderer spriteRenderer;
	private bool rota;

	void Awake()
	{
		col = GetComponent<Collider2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void Romper()
	{
		if (rota) return;
		rota = true;

		col.enabled = false; // Dejamos pasar al jugador mientras la caja se rompe
		StartCoroutine(TitilarYDesaparecer());
	}

	private IEnumerator TitilarYDesaparecer()
	{
		Color original = spriteRenderer.color;
		float fin = Time.time + duracionRotura;
		bool transparente = false;

		while (Time.time < fin)
		{
			transparente = !transparente;
			float alpha = transparente ? 0.3f : 1f;
			spriteRenderer.color = new Color(original.r, original.g, original.b, alpha);
			yield return new WaitForSeconds(0.1f);
		}

		Destroy(gameObject);
	}
}
