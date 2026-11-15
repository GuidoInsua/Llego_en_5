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
// Desc: Charla con el jefe: sortea las excusas según la carrera, las muestra como opciones y decide si te perdona.

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Por cada excusa juntada en la carrera (Resultado_carrera) se sortea una categoría (Sorteo_excusas, pesada por el tiempo) y
// se muestran las mejores, una por botón, con textos de Guion_jefe. La buena convence, la mala no y la intermedia a veces
public class Jefe_dialogo : MonoBehaviour
{
	[SerializeField] private Guion_jefe guion;

	[Header("Escena")]
	[Tooltip("Cuadro con el nombre y el texto del jefe; se oculta mientras se elige.")]
	[SerializeField] private GameObject cuadroDialogo;
	[SerializeField] private Escritura_texto escritura;
	[Tooltip("Llama a Siguiente(); se muestra cuando el jefe termina de hablar.")]
	[SerializeField] private GameObject botonSiguiente;
	[SerializeField] private GameObject panelOpciones;
	[Tooltip("Uno por opción: también es el máximo de excusas a la vista.")]
	[SerializeField] private Button[] botonesOpcion;
	[SerializeField] private GameObject jefeNormal;
	[SerializeField] private GameObject jefeEnojado;

	[Header("Final")]
	[SerializeField] private GameObject pantallaVictoria; // Empieza apagada
	[SerializeField] private TMP_Text textoTiempo;
	[SerializeField] private TMP_Text textoMejorTiempo;
	[SerializeField] private TMP_Text textoExcusas;
	[SerializeField] private GameObject pantallaDespedido; // Empieza apagada

	[Header("Reglas")]
	[Tooltip("Tiempo de carrera (segundos) con el que buenas, intermedias y malas salen con la misma probabilidad.")]
	[SerializeField] private float tiempoEstandar = 50f;
	[Tooltip("Probabilidad que pasa de mala a buena por cada segundo por debajo del estándar (y de buena a mala por encima).")]
	[SerializeField] private float ajustePorSegundo = 0.01f;
	[Tooltip("Probabilidad de que una excusa intermedia convenza al jefe.")]
	[Range(0f, 1f)]
	[SerializeField] private float probabilidadIntermedia = 0.6f;

	[Header("Tiempos")]
	[SerializeField] private float pausaAntesDeHablar = 1f;
	[SerializeField] private float pausaAlTerminarFrase = 0.5f;
	[SerializeField] private float pausaAntesDelFinal = 1f;

	private TMP_Text[] textosOpcion;
	private int opcionElegida;
	private bool isSiguientePedido;

	void Awake()
	{
		if (guion == null || cuadroDialogo == null || escritura == null || botonSiguiente == null || panelOpciones == null ||
			botonesOpcion == null || botonesOpcion.Length == 0 || System.Array.IndexOf(botonesOpcion, null) >= 0 ||
			jefeNormal == null || jefeEnojado == null || pantallaVictoria == null || textoTiempo == null || textoMejorTiempo == null ||
			textoExcusas == null || pantallaDespedido == null)
		{
			Debug.LogError("Jefe_dialogo: falta asignar algo del Inspector.");
			enabled = false;
			return;
		}
		if (!guion.AlcanzanExcusas(botonesOpcion.Length))
		{
			Debug.LogError($"Jefe_dialogo: el guion necesita al menos {botonesOpcion.Length} excusas de cada categoría (una por botón).");
			enabled = false;
			return;
		}

		textosOpcion = new TMP_Text[botonesOpcion.Length];
		for (int i = 0; i < botonesOpcion.Length; i++)
		{
			int indice = i;
			textosOpcion[i] = botonesOpcion[i].GetComponentInChildren<TMP_Text>();
			botonesOpcion[i].onClick.AddListener(() => opcionElegida = indice);
		}
	}

	// Lo llama el botón Siguiente
	public void Siguiente()
	{
		isSiguientePedido = true;
	}

	IEnumerator Start()
	{
		if (!enabled) yield break;

		List<Guion_jefe.Excusa> opciones = SortearOpciones(Resultado_carrera.Excusas);

		yield return new WaitForSeconds(pausaAntesDeHablar);
		cuadroDialogo.SetActive(true);
		yield return Decir(guion.Saludo, true);

		if (opciones.Count == 0)
		{
			yield return Terminar(false, guion.SinExcusas);
			yield break;
		}

		yield return Decir(guion.Pregunta, true);
		yield return Elegir(opciones);

		Categoria_excusa categoria = opciones[opcionElegida].Categoria;
		bool isPerdonado = Sorteo_excusas.Convence(categoria, probabilidadIntermedia, Random.value);
		yield return Terminar(isPerdonado, guion.Respuesta(categoria, isPerdonado));
	}

	// Una categoría por excusa juntada; quedan las mejores (una por botón), cada una con un texto distinto, en orden mezclado
	private List<Guion_jefe.Excusa> SortearOpciones(int juntadas)
	{
		List<Categoria_excusa> categorias = new List<Categoria_excusa>(juntadas);
		for (int i = 0; i < juntadas; i++)
		{
			categorias.Add(Sorteo_excusas.SortearCategoria(Resultado_carrera.Tiempo, tiempoEstandar, ajustePorSegundo, Random.value));
		}
		categorias.Sort((a, b) => (int)b - (int)a);
		if (categorias.Count > botonesOpcion.Length) categorias.RemoveRange(botonesOpcion.Length, categorias.Count - botonesOpcion.Length);

		List<Guion_jefe.Excusa> mazo = new List<Guion_jefe.Excusa>(guion.Excusas);
		Mezclar(mazo);
		List<Guion_jefe.Excusa> opciones = new List<Guion_jefe.Excusa>(categorias.Count);
		foreach (Categoria_excusa categoria in categorias)
		{
			int indice = mazo.FindIndex(excusa => excusa.Categoria == categoria); // Siempre hay: lo comprueba Awake
			opciones.Add(mazo[indice]);
			mazo.RemoveAt(indice);
		}
		Mezclar(opciones); // Que el orden no delate cuál es mejor
		return opciones;
	}

	// Escribe la frase y, si se pide, espera el botón Siguiente
	private IEnumerator Decir(string frase, bool isEsperarSiguiente)
	{
		yield return escritura.Escribir(frase);
		yield return new WaitForSeconds(pausaAlTerminarFrase);

		if (!isEsperarSiguiente) yield break;

		isSiguientePedido = false;
		botonSiguiente.SetActive(true); // Con Seleccion_inicial: queda marcado para Enter
		yield return new WaitUntil(() => isSiguientePedido);
		botonSiguiente.SetActive(false);
	}

	// Un botón por opción (los que sobran se apagan) y espera que se elija uno
	private IEnumerator Elegir(List<Guion_jefe.Excusa> opciones)
	{
		for (int i = 0; i < botonesOpcion.Length; i++)
		{
			bool isUsado = i < opciones.Count;
			botonesOpcion[i].gameObject.SetActive(isUsado);
			if (isUsado) textosOpcion[i].text = opciones[i].Texto;
		}

		cuadroDialogo.SetActive(false);
		opcionElegida = -1;
		panelOpciones.SetActive(true); // Seleccion_inicial marca la primera
		yield return new WaitUntil(() => opcionElegida >= 0);
		panelOpciones.SetActive(false);
		cuadroDialogo.SetActive(true);
	}

	private IEnumerator Terminar(bool isPerdonado, string frase)
	{
		jefeNormal.SetActive(isPerdonado);
		jefeEnojado.SetActive(!isPerdonado);
		yield return Decir(frase, false);
		yield return new WaitForSeconds(pausaAntesDelFinal);

		if (!isPerdonado)
		{
			pantallaDespedido.SetActive(true);
			yield break;
		}

		bool isRecord = Resultado_carrera.GuardarSiEsRecord();
		textoTiempo.text = "Tiempo: " + Cronometro.Formatear(Resultado_carrera.Tiempo);
		textoMejorTiempo.text = "Mejor tiempo: " + Cronometro.Formatear(Resultado_carrera.MejorTiempo) + (isRecord ? " (¡nuevo récord!)" : "");
		textoExcusas.text = "Excusas: " + Resultado_carrera.Excusas;
		pantallaVictoria.SetActive(true);
	}

	private static void Mezclar<T>(List<T> lista)
	{
		for (int i = lista.Count - 1; i > 0; i--)
		{
			int j = Random.Range(0, i + 1);
			(lista[i], lista[j]) = (lista[j], lista[i]);
		}
	}
}
