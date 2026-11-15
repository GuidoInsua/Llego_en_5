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
using System.Collections.Generic;
using UnityEngine;

// Los textos de la charla con el jefe, en un asset para editarlos desde el Inspector sin tocar código
[CreateAssetMenu(fileName = "GuionJefe", menuName = "Llegó en 5/Guion del jefe")]
public class Guion_jefe : ScriptableObject
{
	[Serializable]
	public struct Excusa
	{
		[SerializeField, TextArea] private string texto;
		[SerializeField] private Categoria_excusa categoria;

		public string Texto => texto;
		public Categoria_excusa Categoria => categoria;
	}

	[Header("Lo que dice el jefe")]
	[SerializeField, TextArea] private string saludo;
	[SerializeField, TextArea] private string pregunta;
	[Tooltip("Si no juntaste ninguna excusa en la carrera.")]
	[SerializeField, TextArea] private string sinExcusas;
	[SerializeField, TextArea] private string respuestaBuena;
	[SerializeField, TextArea] private string respuestaIntermediaGana;
	[SerializeField, TextArea] private string respuestaIntermediaPierde;
	[SerializeField, TextArea] private string respuestaMala;

	[Header("Excusas")]
	[SerializeField] private Excusa[] excusas;

	public string Saludo => saludo;
	public string Pregunta => pregunta;
	public string SinExcusas => sinExcusas;
	public IReadOnlyList<Excusa> Excusas => excusas;

	public string Respuesta(Categoria_excusa categoria, bool isPerdonado)
	{
		switch (categoria)
		{
			case Categoria_excusa.Buena: return respuestaBuena;
			case Categoria_excusa.Intermedia: return isPerdonado ? respuestaIntermediaGana : respuestaIntermediaPierde;
			default: return respuestaMala;
		}
	}

	// Para no repetir textos entre las opciones, cada categoría necesita al menos una excusa por botón
	public bool AlcanzanExcusas(int porCategoria)
	{
		if (excusas == null) return false;
		int[] cantidad = new int[3];
		foreach (Excusa excusa in excusas) cantidad[(int)excusa.Categoria]++;
		return cantidad[0] >= porCategoria && cantidad[1] >= porCategoria && cantidad[2] >= porCategoria;
	}
}
