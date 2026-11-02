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
// Desc: Genera y borra los tramos de suelo y siembra las cajas.

using UnityEngine;
using System.Collections.Generic;

public class Ground_spawner : MonoBehaviour
{
	[Header("Configuración del Suelo")]
	[Tooltip("Solo da el estilo de los tramos: la capa, y el material y el color del sprite hijo Road.")]
	[SerializeField] private GameObject groundPrefab;
	[SerializeField] private Transform playerTransform;
	[SerializeField] private float groundY = -1.5f;  // Altura de la superficie del tramo actual; cambia con cada elevación o caída

	[Header("Generación")]
	[SerializeField] private int initialPieces = 4;
	[SerializeField] private float largoInicial = 20f; // Los tramos iniciales son planos de este largo
	[SerializeField] private float distanciaAdelante = 40f; // Cuánto suelo tiene que haber generado por delante del jugador
	[SerializeField] private float despawnDistance = 25f; // Distancia del final del tramo detrás del jugador para destruirlo
	[SerializeField] private float profundidad = 12f; // Cuánto baja el relleno bajo la superficie: más que lo que se ve de la pantalla

	[Header("Tramos")]
	[Tooltip("Largo de cada tramo (mín, máx). Los tramos son planos y quedan separados por un hueco.")]
	[SerializeField] private Vector2 largoTramo = new Vector2(21f, 35f);
	[Tooltip("Ancho del hueco antes de un tramo más alto, fijo.")]
	[SerializeField] private float huecoElevacion = 4f;
	[Tooltip("Ancho del hueco antes de un tramo más bajo, fijo.")]
	[SerializeField] private float huecoCaida = 5f;
	[Tooltip("Cuánto sube el tramo respecto del anterior (mín, máx). Un salto llega a ~2,9 de alto.")]
	[SerializeField] private Vector2 elevacion = new Vector2(0.9f, 1.4f);
	[Tooltip("Cuánto baja el tramo respecto del anterior (mín, máx).")]
	[SerializeField] private Vector2 caida = new Vector2(2f, 3f);
	[Tooltip("Cuántas elevaciones o caídas seguidas puede haber como máximo; después toca la contraria.")]
	[SerializeField] private int maxSeguidas = 2;

	[Header("Obstáculos")]
	[SerializeField] private GameObject cajaPrefab; // Pivote abajo: se apoya sobre el suelo
	[Tooltip("Cantidad de cajas por tramo (mín, máx, inclusive). Salen repartidas en el tramo, a más de margenCaja de los bordes.")]
	[SerializeField] private Vector2Int cajasPorTramo = new Vector2Int(1, 2);
	[Tooltip("Probabilidad de que la caja salga flotando (hay que pasar por abajo deslizándose).")]
	[SerializeField, Range(0f, 1f)] private float probabilidadFlotante = 0.4f;
	[Tooltip("Altura de la base de la caja flotante sobre el suelo: más que alturaDeslizando del jugador (0,8) y menos que su alto parado (1,8).")]
	[SerializeField] private float alturaCajaFlotante = 1.1f;
	[Tooltip("Distancia mínima de las cajas a los bordes del tramo, para que el jugador alcance a reaccionar.")]
	[SerializeField] private float margenCaja = 4f;
	[Tooltip("Distancia mínima entre dos cajas del mismo tramo (un salto cubre ~6 de largo). Si no entra, el tramo lleva menos cajas.")]
	[SerializeField] private float separacionCajas = 8f;

	[Header("Meta")]
	[Tooltip("Al crearse este tramo (contando los iniciales) se avisa dónde va la meta.")]
	[SerializeField] private int tramosParaMeta = 12;

	// Los usa Ajustes_suelo para cambiar los valores en vivo; rigen desde el próximo tramo
	public float LargoMin { get => largoTramo.x; set => largoTramo.x = value; }
	public float LargoMax { get => largoTramo.y; set => largoTramo.y = value; }
	public float HuecoElevacion { get => huecoElevacion; set => huecoElevacion = value; }
	public float HuecoCaida { get => huecoCaida; set => huecoCaida = value; }
	public float ElevacionMin { get => elevacion.x; set => elevacion.x = value; }
	public float ElevacionMax { get => elevacion.y; set => elevacion.y = value; }
	public float CaidaMin { get => caida.x; set => caida.x = value; }
	public float CaidaMax { get => caida.y; set => caida.y = value; }

	// Centro de la superficie del tramo de la meta; lo escucha Meta_victoria
	public event System.Action<Vector2> TramoMetaCreado;

	private struct TramoActivo
	{
		public GameObject objeto;
		public float finX;
	}

	private float nextSpawnX = 0f; // shortcut: float pierde precisión a decenas de miles de unidades (~4 h de juego), si hay modo infinito mover el mundo hacia 0
	private Transform suelo;
	private int capa;
	private bool ultimaFueElevacion;
	private int tramosCreados;
	private int seguidas; // 0 = todavía no hubo elevación ni caída
	private SpriteRenderer estilo; // Sprite, material, color y Sorting Layer que copia cada tramo
	private readonly Queue<TramoActivo> activePieces = new Queue<TramoActivo>();
	private readonly Queue<GameObject> activeCajas = new Queue<GameObject>();

	void Start()
	{
		// Se valida en Start y no en Awake porque el prefab se lee acá, y los tests asignan los campos después de crear el componente
		Transform road = groundPrefab != null ? groundPrefab.transform.Find("Road") : null;
		estilo = road != null ? road.GetComponent<SpriteRenderer>() : null;
		if (playerTransform == null || estilo == null)
		{
			Debug.LogError("Ground_spawner: falta playerTransform o el prefab de suelo sin hijo Road con SpriteRenderer.");
			enabled = false;
			return;
		}

		capa = groundPrefab.layer;

		// Los tramos se fusionan en un solo collider: sin uniones donde la cápsula del jugador se engancha (el "bump")
		GameObject contenedor = new GameObject("Suelo");
		contenedor.layer = capa;
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
		if (playerTransform.position.x + distanciaAdelante > nextSpawnX)
		{
			SpawnPiece(true);
		}

		// Eliminar los tramos que ya quedaron muy atrás de la pantalla
		LimpiarPiezasAtras();
		LimpiarCajasAtras();
	}

	private void SpawnPiece(bool conCaja)
	{
		// Los tramos iniciales son planos y parejos: el jugador arranca sobre suelo firme
		float largo = conCaja ? Random.Range(largoTramo.x, largoTramo.y) : largoInicial;
		if (conCaja)
		{
			bool elevar = seguidas >= maxSeguidas ? !ultimaFueElevacion : Random.value < 0.5f;
			seguidas = seguidas > 0 && elevar == ultimaFueElevacion ? seguidas + 1 : 1;
			ultimaFueElevacion = elevar;

			nextSpawnX += elevar ? huecoElevacion : huecoCaida;
			groundY += elevar ? Random.Range(elevacion.x, elevacion.y) : -Random.Range(caida.x, caida.y);
		}

		activePieces.Enqueue(new TramoActivo { objeto = CrearTramo(new Vector2(nextSpawnX, groundY), largo), finX = nextSpawnX + largo });

		// En el tramo de la meta no hay cajas: ahí va la pared
		tramosCreados++;
		bool esMeta = tramosCreados == tramosParaMeta;
		if (esMeta) TramoMetaCreado?.Invoke(new Vector2(nextSpawnX + largo * 0.5f, groundY));

		// Las cajas van sueltas (no hijas del tramo), con margen en los bordes
		if (conCaja && !esMeta && cajaPrefab != null && largo > 2f * margenCaja)
		{
			// Cada caja cae al azar entre la anterior (más la separación) y lo que deja lugar a las que faltan
			float fin = largo - margenCaja;
			int cantidad = Mathf.Min(Random.Range(cajasPorTramo.x, cajasPorTramo.y + 1), 1 + (int)((fin - margenCaja) / separacionCajas));
			float desde = margenCaja;
			for (int i = 0; i < cantidad; i++)
			{
				float hasta = fin - (cantidad - 1 - i) * separacionCajas;
				float local = Random.Range(desde, hasta);
				desde = local + separacionCajas;
				float x = nextSpawnX + local;
				float y = groundY + (Random.value < probabilidadFlotante ? alturaCajaFlotante : 0f);
				activeCajas.Enqueue(Instantiate(cajaPrefab, new Vector3(x, y, 0f), Quaternion.identity));
			}
		}

		nextSpawnX += largo;
	}

	// Tramo plano: collider (con el origen en la superficie, a la izquierda) y un hijo con el dibujo
	private GameObject CrearTramo(Vector2 origen, float largo)
	{
		GameObject tramo = new GameObject("Tramo");
		tramo.layer = capa;
		tramo.transform.SetParent(suelo, false);
		tramo.transform.position = origen;

		// El dibujo va en un hijo: escalar el tramo deformaría el collider. shortcut: supone el pivote del sprite abajo al centro (como Sprites/Cuadrado)
		GameObject visual = new GameObject("Visual");
		visual.transform.SetParent(tramo.transform, false);
		Vector2 tamano = estilo.sprite.bounds.size;
		visual.transform.localPosition = new Vector3(largo * 0.5f, -profundidad, 0f);
		visual.transform.localScale = new Vector3(largo / tamano.x, profundidad / tamano.y, 1f);
		SpriteRenderer dibujo = visual.AddComponent<SpriteRenderer>();
		dibujo.sprite = estilo.sprite;
		dibujo.sharedMaterial = estilo.sharedMaterial;
		dibujo.color = estilo.color;
		dibujo.sortingLayerID = estilo.sortingLayerID;
		dibujo.sortingOrder = estilo.sortingOrder;

		// El collider sobresale 0,05 en las puntas: se solapa con el vecino y la fusión no deja costura
		PolygonCollider2D col = tramo.AddComponent<PolygonCollider2D>();
		col.SetPath(0, new[]
		{
			new Vector2(-0.05f, 0f), new Vector2(largo + 0.05f, 0f),
			new Vector2(largo + 0.05f, -profundidad), new Vector2(-0.05f, -profundidad)
		});
		col.compositeOperation = Collider2D.CompositeOperation.Merge;

		return tramo;
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

		TramoActivo masViejo = activePieces.Peek();

		// Tramo destruido desde fuera: lo sacamos de la cola para que no bloquee la limpieza
		if (masViejo.objeto == null)
		{
			activePieces.Dequeue();
			return;
		}

		// Si el final del tramo más viejo ya quedó lejos detrás del jugador
		if (playerTransform.position.x - masViejo.finX > despawnDistance)
		{
			activePieces.Dequeue();
			Destroy(masViejo.objeto);
		}
	}
}
