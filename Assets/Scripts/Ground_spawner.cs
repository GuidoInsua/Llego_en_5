using UnityEngine;
using System.Collections.Generic;

public class Ground_spawner : MonoBehaviour
{
	[Header("Configuración del Suelo")]
	[SerializeField] private GameObject groundPrefab;
	[SerializeField] private Transform playerTransform;
	[SerializeField] private float groundY = -2f;    // Altura fija donde se genera el suelo

	[Header("Generación")]
	[SerializeField] private int initialPieces = 4;
	[SerializeField] private float despawnDistance = 25f; // Distancia detrás del jugador para destruir

	[Header("Obstáculos")]
	[SerializeField] private GameObject cajaPrefab; // Pivote abajo: se apoya sobre el suelo
	[SerializeField, Range(0f, 1f)] private float probabilidadCaja = 0.3f;

	// Salen del prefab en Start: así no se desincronizan al retocar el suelo
	private float pieceWidth;  // Ancho del sprite "Road" por la escala
	private float alturaSuelo; // Borde superior del collider respecto a groundY
	private float nextSpawnX = 0f;
	private Transform suelo;
	private readonly Queue<GameObject> activePieces = new Queue<GameObject>();
	private readonly Queue<GameObject> activeCajas = new Queue<GameObject>();

	void Start()
	{
		Vector3 escala = groundPrefab.transform.localScale;
		Transform road = groundPrefab.transform.Find("Road"); // El collider es un poco más ancho (solape), por eso el ancho sale del sprite
		pieceWidth = road.GetComponent<SpriteRenderer>().sprite.bounds.size.x * road.localScale.x * escala.x;
		BoxCollider2D col = groundPrefab.GetComponent<BoxCollider2D>();
		alturaSuelo = (col.offset.y + col.size.y * 0.5f) * escala.y;

		// Las piezas se fusionan en un solo collider: sin uniones donde la cápsula del jugador se engancha (el "bump")
		GameObject contenedor = new GameObject("Suelo");
		contenedor.layer = groundPrefab.layer;
		contenedor.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
		contenedor.AddComponent<CompositeCollider2D>().geometryType = CompositeCollider2D.GeometryType.Outlines;
		suelo = contenedor.transform;

		// Generar los tramos iniciales para que el jugador empiece con suelo bajo los pies
		for (int i = 0; i < initialPieces; i++)
		{
			SpawnPiece(false);
		}
	}

	void Update()
	{
		if (playerTransform == null) return;

		// Si el jugador se acerca al final del suelo generado, crea el siguiente tramo
		if (playerTransform.position.x + (pieceWidth * 2) > nextSpawnX)
		{
			SpawnPiece(true);
		}

		// Eliminar las piezas que ya quedaron muy atrás de la pantalla
		LimpiarPiezasAtras();
		LimpiarCajasAtras();
	}

	private void SpawnPiece(bool conCaja)
	{
		Vector3 spawnPos = new Vector3(nextSpawnX, groundY, 0f);
		GameObject piece = Instantiate(groundPrefab, spawnPos, Quaternion.identity, suelo);

		activePieces.Enqueue(piece);

		// Las cajas van sueltas (no hijas de la pieza) porque la pieza tiene escala distinta a 1
		if (conCaja && cajaPrefab != null && Random.value < probabilidadCaja)
		{
			float x = nextSpawnX + Random.Range(-pieceWidth * 0.25f, pieceWidth * 0.25f);
			activeCajas.Enqueue(Instantiate(cajaPrefab, new Vector3(x, groundY + alturaSuelo, 0f), Quaternion.identity));
		}

		nextSpawnX += pieceWidth;
	}

	private void LimpiarCajasAtras()
	{
		// Cajas rotas (ya destruidas) o muy atrás del jugador
		while (activeCajas.Count > 0 &&
			   (activeCajas.Peek() == null || playerTransform.position.x - activeCajas.Peek().transform.position.x > despawnDistance))
		{
			Destroy(activeCajas.Dequeue());
		}
	}

	private void LimpiarPiezasAtras()
	{
		if (activePieces.Count == 0) return;

		GameObject oldestPiece = activePieces.Peek();

		// Pieza destruida desde fuera: la sacamos de la cola para que no bloquee la limpieza
		if (oldestPiece == null)
		{
			activePieces.Dequeue();
			return;
		}

		// Si la pieza más vieja ya quedó lejos detrás del jugador (se mide desde su centro)
		if (playerTransform.position.x - oldestPiece.transform.position.x > despawnDistance)
		{
			activePieces.Dequeue();
			Destroy(oldestPiece);
		}
	}
}