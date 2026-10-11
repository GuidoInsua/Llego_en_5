using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Comprueba la carrera a velocidad constante y el golpe contra una caja
public class Player_controllerTests
{
	private const float RunSpeed = 7f;
	private const float VelocidadGolpe = 3.5f; // runSpeed * multiplicadorVelocidadGolpe (0.5)

	private GameObject jugador;
	private Rigidbody2D rb;
	private GameObject caja;

	[SetUp]
	public void SetUp()
	{
		jugador = new GameObject("jugador");
		jugador.AddComponent<SpriteRenderer>(); // El golpe titila el sprite
		jugador.AddComponent<BoxCollider2D>().size = Vector2.one;
		Player_controller controller = jugador.AddComponent<Player_controller>(); // Suma Rigidbody2D, Animator y AudioSource

		GameObject groundCheck = new GameObject("groundCheck");
		groundCheck.transform.SetParent(jugador.transform);
		Test_utils.SetField(controller, "groundCheck", groundCheck.transform);
		Test_utils.SetField(controller, "runSpeed", RunSpeed);
		Test_utils.SetField(controller, "duracionGolpe", 0.5f); // Corta para que el test no dure tanto

		rb = jugador.GetComponent<Rigidbody2D>();
		rb.gravityScale = 0f; // Sin gravedad ni suelo: solo medimos el avance horizontal
	}

	[TearDown]
	public void TearDown()
	{
		Object.DestroyImmediate(jugador);
		if (caja != null) Object.DestroyImmediate(caja);
	}

	[UnityTest]
	public IEnumerator RunsAtConstantSpeed()
	{
		yield return new WaitForSeconds(0.5f);

		Assert.AreEqual(RunSpeed, rb.linearVelocity.x, 0.01f);
		Assert.Greater(jugador.transform.position.x, 3f, "en 0.5 s a 7 u/s avanza unas 3.5 unidades");
	}

	[UnityTest]
	public IEnumerator GolpeSlowsPlayerBreaksCajaAndRecovers()
	{
		caja = new GameObject("caja");
		BoxCollider2D cajaCol = caja.AddComponent<BoxCollider2D>();
		cajaCol.size = Vector2.one; // Tamaño explícito: sin sprite el collider no choca con el jugador
		caja.AddComponent<SpriteRenderer>();
		caja.AddComponent<Obstaculo>();
		caja.transform.position = new Vector3(3f, 0f, 0f);
		Physics2D.SyncTransforms(); // AutoSyncTransforms está apagado: sincroniza la posición recién asignada

		float limite = Time.time + 2f;
		while (cajaCol.enabled && Time.time < limite) yield return null;
		Assert.IsFalse(cajaCol.enabled, $"chocar con la caja la rompe (jugador en x={jugador.transform.position.x}, caja bounds={cajaCol.bounds})");

		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		Assert.AreEqual(VelocidadGolpe, rb.linearVelocity.x, 0.01f, "durante el golpe corre más lento");

		yield return new WaitForSeconds(0.8f); // Pasa duracionGolpe (0.5 s)
		Assert.AreEqual(RunSpeed, rb.linearVelocity.x, 0.01f, "después del golpe vuelve a la velocidad normal");
	}
}
