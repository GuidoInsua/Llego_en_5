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
// Desc: Prueba la rotura del obstáculo: deja pasar y se queda.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Comprueba que el obstáculo deja pasar al jugador apenas se rompe y se queda, con su color, al terminar de titilar
public class ObstaculoTests
{
	[UnityTest]
	public IEnumerator RomperDisablesColliderAndStays()
	{
		GameObject go = new GameObject("caja");
		BoxCollider2D col = go.AddComponent<BoxCollider2D>(); // Obstaculo exige Collider2D y SpriteRenderer
		SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
		Obstaculo caja = go.AddComponent<Obstaculo>();
		Test_utils.SetField(caja, "duracionRotura", 0.2f);

		caja.Romper();
		Assert.IsFalse(col.enabled, "el collider se apaga al romperse");

		yield return new WaitForSeconds(0.5f); // Más que duracionRotura
		Assert.IsTrue(go != null, "el obstáculo no desaparece");
		Assert.AreEqual(1f, sr.color.a, "termina de titilar con su color original");
		Object.DestroyImmediate(go);
	}
}
