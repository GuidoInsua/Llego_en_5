using UnityEngine;
using System.Collections.Generic;

public class Ground_spawner : MonoBehaviour
{
	[Header("Configuración del Suelo")]
	[Tooltip("Solo da el estilo de los tramos: la capa, y el material y el color del sprite hijo Road. La forma se arma por código.")]
	[SerializeField] private GameObject groundPrefab;
	[SerializeField] private Transform playerTransform;
	[SerializeField] private float groundY = -1.5f;  // Altura de la superficie donde arranca el suelo; cambia con cada rampa y desnivel

	[Header("Generación")]
	[SerializeField] private int initialPieces = 4;
	[SerializeField] private float largoInicial = 20f; // Los tramos iniciales son planos de este largo
	[SerializeField] private float distanciaAdelante = 40f; // Cuánto suelo tiene que haber generado por delante del jugador
	[SerializeField] private float despawnDistance = 25f; // Distancia del final del tramo detrás del jugador para destruirlo
	[SerializeField] private float profundidad = 12f; // Cuánto baja el relleno bajo la superficie: más que lo que se ve de la pantalla
	[SerializeField] private float pasoMalla = 0.5f; // Separación entre puntos de la superficie en las curvas

	[Header("Formas")]
	[Tooltip("Peso de cada forma, en este orden: Plano, RampaSubida, RampaBajada, Loma, Valle. 0 = no sale.")]
	[SerializeField] private float[] pesosFormas = { 3f, 2f, 2f, 2f, 1.5f };
	[Tooltip("Ningún tramo generado queda más corto que esto (las rampas bajas salían muy cortas).")]
	[SerializeField] private float largoMinimo = 12f;
	[SerializeField] private Vector2 rangoLargoPlano = new Vector2(14f, 22f);
	[Tooltip("Cuánto sube o baja una rampa, o cuánto sobresale una loma o se hunde un valle (mín, máx). Un salto llega a ~2,9 de alto.")]
	[SerializeField] private Vector2 rangoAltura = new Vector2(1.5f, 2.5f);
	[Tooltip("Largo del tramo por cada unidad de altura (mín, máx). Con 2,5 la pendiente máxima es de ~34°. En lomas y valles se duplica.")]
	[SerializeField] private Vector2 rangoLargoPorAltura = new Vector2(2.5f, 4f);

	[Header("Obstáculos")]
	[SerializeField] private GameObject cajaPrefab; // Pivote abajo: se apoya sobre el suelo. Solo salen en tramos planos
	[SerializeField, Range(0f, 1f)] private float probabilidadCaja = 0.5f;

	[Header("Desniveles")]
	[Tooltip("Probabilidad de que un tramo nuevo empiece más abajo, tras un hueco: como saltar de una terraza a otra más baja. En 0 no hay.")]
	[SerializeField, Range(0f, 1f)] private float probabilidadCaida = 0f;
	[Tooltip("Ancho del hueco (mín, máx). El máximo tiene que ser menor que velocidad con golpe × tiempo de la caída más corta (5 × 0,58 s = 2,9): si no, tras un golpe no se llega al tramo de abajo.")]
	[SerializeField] private Vector2 rangoHueco = new Vector2(1.5f, 2f);
	[Tooltip("Cuánto más abajo queda el tramo (mín, máx).")]
	[SerializeField] private Vector2 rangoCaida = new Vector2(5f, 8f);

	private struct TramoActivo
	{
		public GameObject objeto;
		public float finX;
	}

	private float nextSpawnX = 0f; // shortcut: float pierde precisión a decenas de miles de unidades (~4 h de juego), si hay modo infinito mover el mundo hacia 0
	private Transform suelo;
	private int capa;
	private Material material;
	private Color color;
	private readonly Queue<TramoActivo> activePieces = new Queue<TramoActivo>();
	private readonly Queue<GameObject> activeCajas = new Queue<GameObject>();

	void Start()
	{
		// Se valida en Start y no en Awake porque el prefab se lee acá, y los tests asignan los campos después de crear el componente
		Transform road = groundPrefab != null ? groundPrefab.transform.Find("Road") : null;
		SpriteRenderer estilo = road != null ? road.GetComponent<SpriteRenderer>() : null;
		if (playerTransform == null || estilo == null || pesosFormas == null || pesosFormas.Length != System.Enum.GetValues(typeof(Forma_terreno)).Length)
		{
			Debug.LogError("Ground_spawner: falta playerTransform, el prefab de suelo sin hijo Road con SpriteRenderer, o pesosFormas no tiene un peso por forma.");
			enabled = false;
			return;
		}

		capa = groundPrefab.layer;
		material = estilo.sharedMaterial;
		color = estilo.color;

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
		if (conCaja && Random.value < probabilidadCaida)
		{
			nextSpawnX += Random.Range(rangoHueco.x, rangoHueco.y);
			groundY -= Random.Range(rangoCaida.x, rangoCaida.y);
		}

		Forma_terreno forma = conCaja ? (Forma_terreno)Sorteo_ponderado.Elegir(pesosFormas, Random.value) : Forma_terreno.Plano;
		float altura = Random.Range(rangoAltura.x, rangoAltura.y);
		float largo;
		if (!conCaja) largo = largoInicial;
		else if (forma == Forma_terreno.Plano) largo = Random.Range(rangoLargoPlano.x, rangoLargoPlano.y);
		else largo = altura * Random.Range(rangoLargoPorAltura.x, rangoLargoPorAltura.y) * (forma == Forma_terreno.Loma || forma == Forma_terreno.Valle ? 2f : 1f);

		if (conCaja) largo = Mathf.Max(largo, largoMinimo);
		List<Vector2> puntos = Perfil_terreno.Generar(forma, largo, altura, pasoMalla);
		activePieces.Enqueue(new TramoActivo { objeto = CrearTramo(new Vector2(nextSpawnX, groundY), puntos), finX = nextSpawnX + largo });

		// Las cajas van sueltas (no hijas del tramo) y solo sobre tramos planos, donde la base apoya entera
		if (conCaja && forma == Forma_terreno.Plano && cajaPrefab != null && Random.value < probabilidadCaja)
		{
			float x = nextSpawnX + Random.Range(largo * 0.2f, largo * 0.8f);
			activeCajas.Enqueue(Instantiate(cajaPrefab, new Vector3(x, groundY, 0f), Quaternion.identity));
		}

		nextSpawnX += largo;
		groundY += Perfil_terreno.DesnivelFinal(forma, altura);
	}

	// Malla y collider del tramo: la superficie y un relleno hasta "profundidad" más abajo
	private GameObject CrearTramo(Vector2 origen, List<Vector2> superficie)
	{
		GameObject tramo = new GameObject("Tramo");
		tramo.layer = capa;
		tramo.transform.SetParent(suelo, false);
		tramo.transform.position = origen;

		float fondo = superficie[0].y;
		foreach (Vector2 p in superficie) fondo = Mathf.Min(fondo, p.y);
		fondo -= profundidad;

		int n = superficie.Count;
		Vector3[] vertices = new Vector3[n * 2];
		Color[] colores = new Color[n * 2];
		int[] triangulos = new int[(n - 1) * 6];
		for (int i = 0; i < n; i++)
		{
			vertices[2 * i] = superficie[i];
			vertices[2 * i + 1] = new Vector3(superficie[i].x, fondo, 0f);
			colores[2 * i] = colores[2 * i + 1] = color.linear; // Los colores de vértice van en lineal (el SpriteRenderer convierte solo)
			if (i == n - 1) break;

			int a = 2 * i, b = a + 1, c = a + 2, d = a + 3;
			int t = i * 6;
			triangulos[t] = a; triangulos[t + 1] = c; triangulos[t + 2] = b;
			triangulos[t + 3] = c; triangulos[t + 4] = d; triangulos[t + 5] = b;
		}

		Mesh malla = new Mesh { name = "Tramo" };
		malla.vertices = vertices;
		malla.colors = colores;
		malla.triangles = triangulos;
		malla.RecalculateBounds();
		tramo.AddComponent<MeshFilter>().sharedMesh = malla;
		tramo.AddComponent<MeshRenderer>().sharedMaterial = material;

		// El collider sobresale 0,05 en las puntas (la tangente es horizontal ahí): se solapa con el vecino y la fusión no deja costura
		List<Vector2> contorno = new List<Vector2>(n + 4);
		contorno.Add(new Vector2(-0.05f, superficie[0].y));
		contorno.AddRange(superficie);
		contorno.Add(new Vector2(superficie[n - 1].x + 0.05f, superficie[n - 1].y));
		contorno.Add(new Vector2(superficie[n - 1].x + 0.05f, fondo));
		contorno.Add(new Vector2(-0.05f, fondo));
		PolygonCollider2D col = tramo.AddComponent<PolygonCollider2D>();
		col.SetPath(0, contorno);
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
			Destroy(masViejo.objeto.GetComponent<MeshFilter>().sharedMesh);
			Destroy(masViejo.objeto);
		}
	}

	// Los meshes se crean por código: hay que liberarlos al destruir el tramo (Destroy del objeto no los borra)
	void OnDestroy()
	{
		foreach (TramoActivo t in activePieces)
		{
			if (t.objeto != null) Destroy(t.objeto.GetComponent<MeshFilter>().sharedMesh);
		}
	}
}
