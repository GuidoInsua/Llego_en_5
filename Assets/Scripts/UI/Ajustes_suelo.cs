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
// Desc: Panel de prueba con sliders para los valores de generación del suelo.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Panel de prueba con un slider por valor de la generación del suelo. F1 lo muestra u oculta.
[RequireComponent(typeof(Ground_spawner))]
public class Ajustes_suelo : MonoBehaviour
{
	[SerializeField] private Key teclaPanel = Key.F1;

	private const float AltoFila = 50f;
	private GameObject panel;

	void Awake()
	{
		Ground_spawner spawner = GetComponent<Ground_spawner>();
		Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

		GameObject canvasObj = new GameObject("AjustesSuelo", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
		Canvas canvas = canvasObj.GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 50;
		CanvasScaler escala = canvasObj.GetComponent<CanvasScaler>();
		escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		escala.referenceResolution = new Vector2(1920f, 1080f);
		escala.matchWidthOrHeight = 1f;

		panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
		panel.transform.SetParent(canvasObj.transform, false);
		panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
		RectTransform rt = panel.GetComponent<RectTransform>();
		rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
		rt.anchoredPosition = new Vector2(20f, -20f);

		// nombre, mínimo, máximo, valor actual, cómo se aplica
		(string, float, float, float, UnityEngine.Events.UnityAction<float>)[] filas =
		{
			("Largo mín", 4f, 40f, spawner.LargoMin, v => spawner.LargoMin = v),
			("Largo máx", 4f, 40f, spawner.LargoMax, v => spawner.LargoMax = v),
			("Distancia (elevación)", 0f, 8f, spawner.HuecoElevacion, v => spawner.HuecoElevacion = v),
			("Distancia (caída)", 0f, 8f, spawner.HuecoCaida, v => spawner.HuecoCaida = v),
			("Elevación mín", 0f, 5f, spawner.ElevacionMin, v => spawner.ElevacionMin = v),
			("Elevación máx", 0f, 5f, spawner.ElevacionMax, v => spawner.ElevacionMax = v),
			("Caída mín", 0f, 12f, spawner.CaidaMin, v => spawner.CaidaMin = v),
			("Caída máx", 0f, 12f, spawner.CaidaMax, v => spawner.CaidaMax = v),
		};
		rt.sizeDelta = new Vector2(520f, filas.Length * AltoFila + 20f);

		for (int i = 0; i < filas.Length; i++)
		{
			var (nombre, min, max, valor, aplicar) = filas[i];
			float y = -10f - i * AltoFila;

			Text texto = CrearTexto(fuente, new Vector2(10f, y), new Vector2(500f, 24f));
			Slider slider = CrearSlider(new Vector2(10f, y - 26f), new Vector2(500f, 18f));
			slider.minValue = min;
			slider.maxValue = max;
			slider.onValueChanged.AddListener(v => { aplicar(v); texto.text = nombre + ": " + v.ToString("0.0"); });
			slider.value = valor; // Dispara el listener y escribe el texto
		}
	}

	void Update()
	{
		if (Keyboard.current != null && Keyboard.current[teclaPanel].wasPressedThisFrame) panel.SetActive(!panel.activeSelf);
	}

	private Text CrearTexto(Font fuente, Vector2 pos, Vector2 tam)
	{
		Text t = new GameObject("Texto", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
		t.font = fuente;
		t.fontSize = 20;
		t.color = Color.white;
		Colocar(t.rectTransform, pos, tam);
		return t;
	}

	// Sin sprites: las imágenes quedan como rectángulos lisos, suficiente para un panel de prueba
	private Slider CrearSlider(Vector2 pos, Vector2 tam)
	{
		GameObject raiz = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
		Colocar(raiz.GetComponent<RectTransform>(), pos, tam);

		RectTransform fondo = CrearImagen("Fondo", raiz.transform, new Color(1f, 1f, 1f, 0.25f));
		fondo.anchorMin = Vector2.zero;
		fondo.anchorMax = Vector2.one;
		fondo.sizeDelta = Vector2.zero;

		RectTransform area = new GameObject("Area", typeof(RectTransform)).GetComponent<RectTransform>();
		area.SetParent(raiz.transform, false);
		area.anchorMin = Vector2.zero;
		area.anchorMax = Vector2.one;
		area.sizeDelta = new Vector2(-20f, 0f); // Deja lugar a medio tirador a cada lado

		RectTransform tirador = CrearImagen("Tirador", area, Color.white);
		tirador.sizeDelta = new Vector2(20f, 0f);

		Slider slider = raiz.GetComponent<Slider>();
		slider.handleRect = tirador;
		slider.targetGraphic = tirador.GetComponent<Image>();
		return slider;
	}

	private static RectTransform CrearImagen(string nombre, Transform padre, Color color)
	{
		GameObject obj = new GameObject(nombre, typeof(RectTransform), typeof(Image));
		obj.transform.SetParent(padre, false);
		obj.GetComponent<Image>().color = color;
		return obj.GetComponent<RectTransform>();
	}

	// Cada control cuelga de la esquina de arriba a la izquierda del panel
	private void Colocar(RectTransform rt, Vector2 pos, Vector2 tam)
	{
		rt.SetParent(panel.transform, false);
		rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
		rt.anchoredPosition = pos;
		rt.sizeDelta = tam;
	}
}
