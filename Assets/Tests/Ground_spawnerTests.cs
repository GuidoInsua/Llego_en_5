using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Comprueba que el suelo y las cajas aparecen delante del jugador y desaparecen detrás
public class Ground_spawnerTests
{
	private GameObject spawnerObject;
	private GameObject playerObject;
	private GameObject piecePrefab;
	private GameObject cajaPrefab;
	private Ground_spawner spawner;

	[SetUp]
	public void SetUp()
	{
		piecePrefab = new GameObject("piece");
		playerObject = new GameObject("player");

		spawnerObject = new GameObject("spawner");
		spawner = spawnerObject.AddComponent<Ground_spawner>();
		Test_utils.SetField(spawner, "groundPrefab", piecePrefab);
		Test_utils.SetField(spawner, "playerTransform", playerObject.transform);
	}

	[TearDown]
	public void TearDown()
	{
		// Inmediato: si no, los restos del test anterior se cuentan en el siguiente
		foreach (Transform t in Buscar("piece(Clone)")) Object.DestroyImmediate(t.gameObject);
		foreach (Transform t in Buscar("caja(Clone)")) Object.DestroyImmediate(t.gameObject);
		Object.DestroyImmediate(spawnerObject);
		Object.DestroyImmediate(playerObject);
		Object.DestroyImmediate(piecePrefab);
		if (cajaPrefab != null) Object.DestroyImmediate(cajaPrefab);
	}

	[UnityTest]
	public IEnumerator SpawnsAheadAndRemovesBehind()
	{
		yield return Esperar(2); // Start crea las 4 piezas iniciales en x = 0, 20, 40, 60
		CollectionAssert.AreEqual(new float[] { 0f, 20f, 40f, 60f }, PosicionesX("piece(Clone)"));

		playerObject.transform.position = new Vector3(50f, 0f, 0f);
		// Crea la pieza en x = 80 y quita de a una por frame las que quedan a más de 25 unidades: x = 0 y x = 20
		yield return Esperar(4);

		CollectionAssert.AreEqual(new float[] { 40f, 60f, 80f }, PosicionesX("piece(Clone)"));
	}

	[UnityTest]
	public IEnumerator SpawnsCajaOnlyOnNewPieces()
	{
		cajaPrefab = new GameObject("caja");
		Test_utils.SetField(spawner, "cajaPrefab", cajaPrefab);
		Test_utils.SetField(spawner, "probabilidadCaja", 1f);

		yield return Esperar(2);
		Assert.AreEqual(0, Buscar("caja(Clone)").Length, "las piezas iniciales no llevan caja");

		playerObject.transform.position = new Vector3(50f, 0f, 0f);
		yield return Esperar(2);

		Transform[] cajas = Buscar("caja(Clone)");
		Assert.AreEqual(1, cajas.Length);
		// Pieza nueva en x = 80, la caja cae dentro de +-5 y apoyada sobre el suelo (groundY -2 + alturaSuelo 0.5)
		Assert.That(cajas[0].position.x, Is.InRange(75f, 85f));
		Assert.AreEqual(-1.5f, cajas[0].position.y, 0.001f);
	}

	private static IEnumerator Esperar(int frames)
	{
		for (int i = 0; i < frames; i++) yield return null;
	}

	private static float[] PosicionesX(string nombre)
	{
		return Buscar(nombre).Select(t => t.position.x).OrderBy(x => x).ToArray();
	}

	private static Transform[] Buscar(string nombre)
	{
		return Object.FindObjectsByType<Transform>()
			.Where(t => t.name == nombre)
			.ToArray();
	}
}
