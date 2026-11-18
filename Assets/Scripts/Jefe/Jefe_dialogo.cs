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

// Por cada excusa juntada en la carrera (Resultado_carrera) se sortea una categoría (Sorteo_excusas, según el tiempo) y
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
	private int opcionElegida; // -1 mientras no se eligió ninguna
	private bool isSiguientePedido;

	void Awake()
	{
		bool isFaltaAlgo = guion == null || cuadroDialogo == null || escritura == null || botonSiguiente == null || panelOpciones == null ||
			jefeNormal == null || jefeEnojado == null || pantallaVictoria == null || textoTiempo == null || textoMejorTiempo == null ||
			textoExcusas == null || pantallaDespedido == null;

		if (botonesOpcion == null || botonesOpcion.Length == 0)
		{
			isFaltaAlgo = true;
		}
		else
		{
			foreach (Button boton in botonesOpcion)
			{
				if (boton == null)
				{
					isFaltaAlgo = true;
				}
			}
		}

		if (isFaltaAlgo)
		{
			Debug.LogError("Jefe_dialogo: falta asignar algo del Inspector.");
			enabled = false;
			return;
		}

		if (!guion.AlcanzanExcusas(botonesOpcion.Length))
		{
			Debug.LogError("Jefe_dialogo: el guion necesita al menos " + botonesOpcion.Length + " excusas de cada categoría (una por botón).");
			enabled = false;
			return;
		}

		textosOpcion = new TMP_Text[botonesOpcion.Length];
		for (int i = 0; i < botonesOpcion.Length; i++)
		{
			textosOpcion[i] = botonesOpcion[i].GetComponentInChildren<TMP_Text>();

			// Al apretar el botón se guarda su número. El botón no sabe pasar un número, así que se usa una
			// función sin nombre (lambda): se acuerda del valor de `indice` de esta vuelta del for
			int indice = i;
			botonesOpcion[i].onClick.AddListener(() => opcionElegida = indice);
		}
	}

	// Lo llama el botón Siguiente
	public void Siguiente()
	{
		isSiguientePedido = true;
	}

	// Start puede ser una corrutina: la charla entera va acá, paso a paso
	IEnumerator Start()
	{
		if (!enabled)
		{
			yield break;
		}

		List<Guion_jefe.Excusa> opciones = SortearOpciones(Resultado_carrera.Excusas);

		yield return new WaitForSeconds(pausaAntesDeHablar);
		cuadroDialogo.SetActive(true);
		yield return Decir(guion.Saludo, true);

		// Sin excusas el jefe te despide sin preguntar
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
		// 1. Una categoría al azar por cada excusa juntada
		List<Categoria_excusa> categorias = new List<Categoria_excusa>();
		for (int i = 0; i < juntadas; i++)
		{
			Categoria_excusa categoria = Sorteo_excusas.SortearCategoria(Resultado_carrera.Tiempo, tiempoEstandar, ajustePorSegundo, Random.value);
			categorias.Add(categoria);
		}

		// 2. De mejor a peor: Sort las ordena por su número (Mala 0 ... Buena 2) de menor a mayor y Reverse da vuelta la lista
		categorias.Sort();
		categorias.Reverse();

		// 3. Se queda con las primeras, una por botón
		if (categorias.Count > botonesOpcion.Length)
		{
			int sobran = categorias.Count - botonesOpcion.Length;
			categorias.RemoveRange(botonesOpcion.Length, sobran);
		}

		// 4. Para cada categoría saca del mazo mezclado una excusa de esa categoría (así no se repiten textos)
		List<Guion_jefe.Excusa> mazo = new List<Guion_jefe.Excusa>(guion.Excusas);
		Mezclar(mazo);
		List<Guion_jefe.Excusa> opciones = new List<Guion_jefe.Excusa>();
		foreach (Categoria_excusa categoria in categorias)
		{
			int indice = BuscarExcusa(mazo, categoria); // Siempre encuentra: lo comprueba Awake
			opciones.Add(mazo[indice]);
			mazo.RemoveAt(indice);
		}

		// 5. Se mezclan para que el orden no delate cuál es mejor
		Mezclar(opciones);
		return opciones;
	}

	// Posición de la primera excusa de esa categoría en la lista, o -1 si no hay
	private int BuscarExcusa(List<Guion_jefe.Excusa> lista, Categoria_excusa categoria)
	{
		for (int i = 0; i < lista.Count; i++)
		{
			if (lista[i].Categoria == categoria)
			{
				return i;
			}
		}
		return -1;
	}

	// Escribe la frase y, si se pide, espera el botón Siguiente
	private IEnumerator Decir(string frase, bool isEsperarSiguiente)
	{
		yield return escritura.Escribir(frase);
		yield return new WaitForSeconds(pausaAlTerminarFrase);

		if (!isEsperarSiguiente)
		{
			yield break;
		}

		isSiguientePedido = false;
		botonSiguiente.SetActive(true); // Con Seleccion_inicial: queda marcado para Enter
		while (!isSiguientePedido)
		{
			yield return null;
		}
		botonSiguiente.SetActive(false);
	}

	// Un botón por opción (los que sobran se apagan) y espera que se elija uno
	private IEnumerator Elegir(List<Guion_jefe.Excusa> opciones)
	{
		for (int i = 0; i < botonesOpcion.Length; i++)
		{
			bool isUsado = i < opciones.Count;
			botonesOpcion[i].gameObject.SetActive(isUsado);
			if (isUsado)
			{
				textosOpcion[i].text = opciones[i].Texto;
			}
		}

		cuadroDialogo.SetActive(false);
		opcionElegida = -1;
		panelOpciones.SetActive(true); // Seleccion_inicial marca la primera
		while (opcionElegida < 0)
		{
			yield return null;
		}
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
		string textoMejor = "Mejor tiempo: " + Cronometro.Formatear(Resultado_carrera.MejorTiempo);
		if (isRecord)
		{
			textoMejor = textoMejor + " (¡nuevo récord!)";
		}

		textoTiempo.text = "Tiempo: " + Cronometro.Formatear(Resultado_carrera.Tiempo);
		textoMejorTiempo.text = textoMejor;
		textoExcusas.text = "Excusas: " + Resultado_carrera.Excusas;
		pantallaVictoria.SetActive(true);
	}

	// Mezcla la lista al azar: de atrás para adelante, cambia cada elemento por uno al azar de los que tiene antes (o por sí mismo)
	private void Mezclar(List<Guion_jefe.Excusa> lista)
	{
		for (int i = lista.Count - 1; i > 0; i--)
		{
			int j = Random.Range(0, i + 1);
			Guion_jefe.Excusa guardada = lista[i];
			lista[i] = lista[j];
			lista[j] = guardada;
		}
	}
}
