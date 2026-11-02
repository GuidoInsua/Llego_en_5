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
// Desc: Prueba la generación de tramos y cajas del suelo.

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
		// El prefab solo da el estilo: la capa y el sprite hijo "Road" (material y color)
		piecePrefab = new GameObject("piece");
		piecePrefab.layer = 3;
		GameObject road = new GameObject("Road");
		road.transform.SetParent(piecePrefab.transform);
		textura = new Texture2D(2000, 1);
		road.AddComponent<SpriteRenderer>().sprite = Sprite.Create(textura, new Rect(0, 0, 2000, 1), new Vector2(0.5f, 0.5f), 100f);

		playerObject = new GameObject("player");

		spawnerObject = new GameObject("spawner");
		spawner = spawnerObject.AddComponent<Ground_spawner>();
		Test_utils.SetField(spawner, "groundPrefab", piecePrefab);
		Test_utils.SetField(spawner, "playerTransform", playerObject.transform);
		// Tramos de 20 y sin hueco: así las posiciones salen exactas
		Test_utils.SetField(spawner, "largoTramo", new Vector2(20f, 20f));
		Test_utils.SetField(spawner, "huecoElevacion", 0f);
		Test_utils.SetField(spawner, "huecoCaida", 0f);
		Test_utils.SetField(spawner, "probabilidadFlotante", 0f);
		Test_utils.SetField(spawner, "elevacion", Vector2.zero);
		Test_utils.SetField(spawner, "caida", Vector2.zero);
	}

	[TearDown]
	public void TearDown()
	{
		// Inmediato: si no, los restos del test anterior se cuentan en el siguiente
		foreach (Transform t in Buscar("Tramo")) Object.DestroyImmediate(t.gameObject);
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
		yield return Esperar(2); // Start crea los 4 tramos iniciales en x = 0, 20, 40, 60
		CollectionAssert.AreEqual(new float[] { 0f, 20f, 40f, 60f }, PosicionesX("Tramo"));

		playerObject.transform.position = new Vector3(50f, 0f, 0f);
		// Crea el tramo en x = 80 y quita el que termina a más de 25 unidades detrás (el de x = 0, que termina en 20)
		yield return Esperar(4);

		CollectionAssert.AreEqual(new float[] { 20f, 40f, 60f, 80f }, PosicionesX("Tramo"));
	}

	[UnityTest]
	public IEnumerator SpawnsCajaOnlyOnNewPieces()
	{
		cajaPrefab = new GameObject("caja");
		Test_utils.SetField(spawner, "cajaPrefab", cajaPrefab);
		Test_utils.SetField(spawner, "cajasPorTramo", new Vector2Int(1, 1));

		yield return Esperar(2);
		Assert.AreEqual(0, Buscar("caja(Clone)").Length, "las piezas iniciales no llevan caja");

		playerObject.transform.position = new Vector3(50f, 0f, 0f);
		yield return Esperar(2);

		Transform[] cajas = Buscar("caja(Clone)");
		Assert.AreEqual(1, cajas.Length);
		// Tramo nuevo en x = 80..100, la caja cae en su parte central y apoyada sobre el suelo (groundY -1,5)
		Assert.That(cajas[0].position.x, Is.InRange(84f, 96f));
		Assert.AreEqual(-1.5f, cajas[0].position.y, 0.001f);
	}

	[UnityTest]
	public IEnumerator HuecoFijoYDesnivelRespetaElLimite()
	{
		Test_utils.SetField(spawner, "huecoElevacion", 2f);
		Test_utils.SetField(spawner, "huecoCaida", 2f);
		Test_utils.SetField(spawner, "elevacion", new Vector2(3f, 3f));
		Test_utils.SetField(spawner, "caida", new Vector2(5f, 5f));

		yield return Esperar(2);
		playerObject.transform.position = new Vector3(50f, 0f, 0f);
		yield return Esperar(2);

		// Tramo nuevo: 80 + hueco de 2, y 3 más arriba o 5 más abajo que el suelo inicial (-1,5)
		Transform nueva = Buscar("Tramo").OrderBy(t => t.position.x).Last();
		Assert.AreEqual(82f, nueva.position.x, 0.001f);
		Assert.That(nueva.position.y, Is.EqualTo(1.5f).Within(0.001f).Or.EqualTo(-6.5f).Within(0.001f));
	}

	[UnityTest]
	public IEnumerator PiezasSeFusionanEnUnSoloCollider()
	{
		yield return Esperar(2);
		yield return new WaitForFixedUpdate();

		// Sin uniones entre tramos: si se cortara en varios contornos vuelve el "bump" de la cápsula
		CompositeCollider2D suelo = Buscar("Suelo")[0].GetComponent<CompositeCollider2D>();
		Assert.AreEqual(1, suelo.pathCount, "los 4 tramos forman un solo contorno");
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
