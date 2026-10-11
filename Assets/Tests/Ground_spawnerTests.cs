using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Comprueba que el suelo aparece delante del jugador y desaparece detrás
public class Ground_spawnerTests
{
	private GameObject spawnerObject;
	private GameObject playerObject;
	private GameObject piecePrefab;

	[SetUp]
	public void SetUp()
	{
		piecePrefab = new GameObject("piece");
		playerObject = new GameObject("player");

		spawnerObject = new GameObject("spawner");
		Ground_spawner spawner = spawnerObject.AddComponent<Ground_spawner>();
		SetField(spawner, "groundPrefab", piecePrefab);
		SetField(spawner, "playerTransform", playerObject.transform);
	}

	[TearDown]
	public void TearDown()
	{
		foreach (Transform piece in GetPieces()) Object.Destroy(piece.gameObject);
		Object.Destroy(spawnerObject);
		Object.Destroy(playerObject);
		Object.Destroy(piecePrefab);
	}

	[UnityTest]
	public IEnumerator SpawnsAheadAndRemovesBehind()
	{
		yield return null; // Start crea las 4 piezas iniciales en x = 0, 20, 40, 60

		playerObject.transform.position = new Vector3(50f, 0f, 0f);
		yield return null; // Update crea la pieza en x = 80 y quita la de x = 0

		float[] xs = GetPieces().Select(p => p.position.x).OrderBy(x => x).ToArray();
		CollectionAssert.AreEqual(new float[] { 20f, 40f, 60f, 80f }, xs);
	}

	private static Transform[] GetPieces()
	{
		return Object.FindObjectsByType<Transform>()
			.Where(t => t.name == "piece(Clone)")
			.ToArray();
	}

	private static void SetField(object target, string name, object value)
	{
		target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
	}
}
