using UnityEngine;
using System.Collections.Generic;

public class Ground_spawner : MonoBehaviour
{
	[Header("Configuración del Suelo")]
	[SerializeField] private GameObject groundPrefab;
	[SerializeField] private Transform playerTransform;
	[SerializeField] private float pieceWidth = 20f; // Mismo valor que el Scale X del prefab
	[SerializeField] private float groundY = -2f;    // Altura fija donde se genera el suelo

	[Header("Generación")]
	[SerializeField] private int initialPieces = 4;
	[SerializeField] private float despawnDistance = 25f; // Distancia detrás del jugador para destruir

	private float nextSpawnX = 0f;
	private readonly Queue<GameObject> activePieces = new Queue<GameObject>();

	void Start()
	{
		// Generar los tramos iniciales para que el jugador empiece con suelo bajo los pies
		for (int i = 0; i < initialPieces; i++)
		{
			SpawnPiece();
		}
	}

	void Update()
	{
		if (playerTransform == null) return;

		// Si el jugador se acerca al final del suelo generado, crea el siguiente tramo
		if (playerTransform.position.x + (pieceWidth * 2) > nextSpawnX)
		{
			SpawnPiece();
		}

		// Eliminar las piezas que ya quedaron muy atrás de la pantalla
		LimpiarPiezasAtras();
	}

	private void SpawnPiece()
	{
		Vector3 spawnPos = new Vector3(nextSpawnX, groundY, 0f);
		GameObject piece = Instantiate(groundPrefab, spawnPos, Quaternion.identity);

		activePieces.Enqueue(piece);
		nextSpawnX += pieceWidth;
	}

	private void LimpiarPiezasAtras()
	{
		if (activePieces.Count == 0) return;

		GameObject oldestPiece = activePieces.Peek();

		// Si el borde derecho de la pieza más vieja ya quedó detrás del jugador
		if (oldestPiece != null && playerTransform.position.x - oldestPiece.transform.position.x > despawnDistance)
		{
			activePieces.Dequeue();
			Destroy(oldestPiece);
		}
	}
}