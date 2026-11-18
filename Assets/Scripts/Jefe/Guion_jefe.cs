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
// Desc: Todo lo que dice el jefe y la lista de excusas con su categoría (asset Data/GuionJefe).

using System;
using UnityEngine;

// Los textos de la charla con el jefe, en un asset para editarlos desde el Inspector sin tocar código
[CreateAssetMenu(fileName = "GuionJefe", menuName = "Llegó en 5/Guion del jefe")]
public class Guion_jefe : ScriptableObject
{
	// Una excusa: su texto y qué tan buena es. [Serializable] hace que se pueda editar en el Inspector
	[Serializable]
	public struct Excusa
	{
		[SerializeField] [TextArea] private string texto;
		[SerializeField] private Categoria_excusa categoria;

		public string Texto
		{
			get { return texto; }
		}

		public Categoria_excusa Categoria
		{
			get { return categoria; }
		}
	}

	[Header("Lo que dice el jefe")]
	[SerializeField] [TextArea] private string saludo;
	[SerializeField] [TextArea] private string pregunta;
	[Tooltip("Si no juntaste ninguna excusa en la carrera.")]
	[SerializeField] [TextArea] private string sinExcusas;
	[SerializeField] [TextArea] private string respuestaBuena;
	[SerializeField] [TextArea] private string respuestaIntermediaGana;
	[SerializeField] [TextArea] private string respuestaIntermediaPierde;
	[SerializeField] [TextArea] private string respuestaMala;

	[Header("Excusas")]
	[SerializeField] private Excusa[] excusas;

	public string Saludo
	{
		get { return saludo; }
	}

	public string Pregunta
	{
		get { return pregunta; }
	}

	public string SinExcusas
	{
		get { return sinExcusas; }
	}

	public Excusa[] Excusas
	{
		get { return excusas; }
	}

	// Lo que contesta el jefe a la excusa elegida
	public string Respuesta(Categoria_excusa categoria, bool isPerdonado)
	{
		if (categoria == Categoria_excusa.Buena)
		{
			return respuestaBuena;
		}

		if (categoria == Categoria_excusa.Intermedia)
		{
			if (isPerdonado)
			{
				return respuestaIntermediaGana;
			}
			else
			{
				return respuestaIntermediaPierde;
			}
		}

		return respuestaMala;
	}

	// Para no repetir textos entre las opciones, cada categoría necesita al menos una excusa por botón
	public bool AlcanzanExcusas(int porCategoria)
	{
		if (excusas == null)
		{
			return false;
		}

		int malas = 0;
		int intermedias = 0;
		int buenas = 0;
		foreach (Excusa excusa in excusas)
		{
			if (excusa.Categoria == Categoria_excusa.Mala)
			{
				malas++;
			}
			else if (excusa.Categoria == Categoria_excusa.Intermedia)
			{
				intermedias++;
			}
			else
			{
				buenas++;
			}
		}

		return malas >= porCategoria && intermedias >= porCategoria && buenas >= porCategoria;
	}
}
