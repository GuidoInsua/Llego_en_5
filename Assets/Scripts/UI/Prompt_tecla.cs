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
// Desc: Recuadro con una letra en pantalla que avisa qué tecla apretar (armado por código).

using UnityEngine;
using UnityEngine.UI;

// Cuadrado blanco con la letra de la tecla, en el centro de la pantalla. Lo muestran los hidrantes (Chorro_agua)
public class Prompt_tecla : MonoBehaviour
{
	[SerializeField] private string letra = "E";
	[SerializeField] private float lado = 160f; // En unidades del CanvasScaler (referencia 1920×1080)
	[Tooltip("Altura en pantalla: 0 abajo, 1 arriba.")]
	[SerializeField, Range(0f, 1f)] private float alturaPantalla = 0.7f;

	private GameObject recuadro;

	void Awake()
	{
		Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

		GameObject canvasObj = new GameObject("PromptTecla", typeof(Canvas), typeof(CanvasScaler));
		canvasObj.transform.SetParent(transform, false);
		Canvas canvas = canvasObj.GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 40;
		CanvasScaler escala = canvasObj.GetComponent<CanvasScaler>();
		escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		escala.referenceResolution = new Vector2(1920f, 1080f);
		escala.matchWidthOrHeight = 1f;

		recuadro = new GameObject("Recuadro", typeof(RectTransform), typeof(Image), typeof(Outline));
		recuadro.transform.SetParent(canvasObj.transform, false);
		recuadro.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
		recuadro.GetComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);
		recuadro.GetComponent<Outline>().effectDistance = new Vector2(6f, -6f);
		RectTransform rt = recuadro.GetComponent<RectTransform>();
		rt.anchorMin = rt.anchorMax = new Vector2(0.5f, alturaPantalla);
		rt.sizeDelta = new Vector2(lado, lado);

		GameObject textoObj = new GameObject("Letra", typeof(RectTransform), typeof(Text));
		textoObj.transform.SetParent(recuadro.transform, false);
		Text texto = textoObj.GetComponent<Text>();
		texto.font = fuente;
		texto.text = letra;
		texto.fontSize = Mathf.RoundToInt(lado * 0.7f);
		texto.fontStyle = FontStyle.Bold;
		texto.alignment = TextAnchor.MiddleCenter;
		texto.color = Color.black;
		RectTransform rtTexto = texto.rectTransform;
		rtTexto.anchorMin = Vector2.zero;
		rtTexto.anchorMax = Vector2.one;
		rtTexto.sizeDelta = Vector2.zero;

		recuadro.SetActive(false);
	}

	public void Mostrar()
	{
		recuadro.SetActive(true);
	}

	public void Ocultar()
	{
		recuadro.SetActive(false);
	}
}
