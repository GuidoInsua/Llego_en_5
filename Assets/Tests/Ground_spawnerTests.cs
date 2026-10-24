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
	private Texture2D textura;
	private Ground_spawner spawner;

	[SetUp]
	public void SetUp()
	{
		// Pieza como el prefab real: el spawner saca el ancho del sprite "Road" (2000 px a 100 PPU = 20)
		// y la altura del borde superior del collider (0.5)
		piecePrefab = new GameObject("piece");
		piecePrefab.layer = 3;
		BoxCollider2D col = piecePrefab.AddComponent<BoxCollider2D>();
		col.size = new Vector2(20.1f, 1f); // Un poco más ancho que la pieza, como el real
		col.compositeOperation = Collider2D.CompositeOperation.Merge;
		GameObject road = new GameObject("Road");
		road.transform.SetParent(piecePrefab.transform);
		textura = new Texture2D(2000, 1);
		road.AddComponent<SpriteRenderer>().sprite = Sprite.Create(textura, new Rect(0, 0, 2000, 1), new Vector2(0.5f, 0.5f), 100f);

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
		foreach (Transform t in Buscar("Suelo")) Object.DestroyImmediate(t.gameObject);
		Object.DestroyImmediate(spawnerObject);
		Object.DestroyImmediate(playerObject);
		Object.DestroyImmediate(piecePrefab);
		Object.DestroyImmediate(textura);
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

	[UnityTest]
	public IEnumerator DesnivelDejaHuecoYBajaElTramo()
	{
		Test_utils.SetField(spawner, "probabilidadCaida", 1f);
		Test_utils.SetField(spawner, "rangoHueco", new Vector2(2f, 2f));
		Test_utils.SetField(spawner, "rangoCaida", new Vector2(5f, 5f));

		yield return Esperar(2);
		playerObject.transform.position = new Vector3(50f, 0f, 0f);
		yield return Esperar(2);

		// Pieza nueva: 80 + hueco de 2, y 5 más abajo que el suelo inicial (-2)
		Transform nueva = Buscar("piece(Clone)").OrderBy(t => t.position.x).Last();
		Assert.AreEqual(82f, nueva.position.x, 0.001f);
		Assert.AreEqual(-7f, nueva.position.y, 0.001f);
	}

	[UnityTest]
	public IEnumerator PiezasSeFusionanEnUnSoloCollider()
	{
		yield return Esperar(2);
		yield return new WaitForFixedUpdate();

		// Sin uniones entre tramos: si se cortara en varios contornos vuelve el "bump" de la cápsula
		CompositeCollider2D suelo = Buscar("Suelo")[0].GetComponent<CompositeCollider2D>();
		Assert.AreEqual(1, suelo.pathCount, "las 4 piezas forman un solo contorno");
		Assert.AreEqual(3, suelo.gameObject.layer, "el suelo toma la capa del prefab, que es la que detecta GroundCheck");
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
