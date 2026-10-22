using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Comprueba que la caja deja pasar al jugador apenas se rompe y desaparece después
public class ObstaculoTests
{
	[UnityTest]
	public IEnumerator RomperDisablesColliderAndDestroysAfterDuration()
	{
		GameObject go = new GameObject("caja");
		BoxCollider2D col = go.AddComponent<BoxCollider2D>(); // Obstaculo exige Collider2D y SpriteRenderer
		go.AddComponent<SpriteRenderer>();
		Obstaculo caja = go.AddComponent<Obstaculo>();
		Test_utils.SetField(caja, "duracionRotura", 0.2f);

		caja.Romper();
		Assert.IsFalse(col.enabled, "el collider se apaga al romperse");

		float limite = Time.time + 3f; // Espera la condición, no un tiempo fijo
		while (go != null && Time.time < limite) yield return null;
		Assert.IsTrue(go == null, "la caja se destruye al terminar de titilar");
	}
}
