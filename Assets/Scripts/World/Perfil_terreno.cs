using System.Collections.Generic;
using UnityEngine;

// El orden es el de los pesos de Ground_spawner
public enum Forma_terreno
{
	Plano,
	RampaSubida,
	RampaBajada,
	Loma,
	Valle
}

// Superficie de un tramo de suelo como lista de puntos. Todas las formas empiezan y terminan con la tangente horizontal,
// así los tramos se unen sin quiebre y la pendiente máxima queda en el medio (1,5 × altura / largo en las rampas)
public static class Perfil_terreno
{
	// Puntos desde (0, 0) hasta (largo, DesnivelFinal). paso = separación horizontal máxima entre puntos
	public static List<Vector2> Generar(Forma_terreno forma, float largo, float altura, float paso)
	{
		int tramos = forma == Forma_terreno.Plano ? 1 : Mathf.Max(2, Mathf.CeilToInt(largo / paso));
		List<Vector2> puntos = new List<Vector2>(tramos + 1);
		for (int i = 0; i <= tramos; i++)
		{
			float t = (float)i / tramos;
			puntos.Add(new Vector2(largo * t, Altura(forma, t, altura)));
		}

		return puntos;
	}

	// Cuánto más arriba (o abajo) termina el tramo respecto de donde empieza
	public static float DesnivelFinal(Forma_terreno forma, float altura)
	{
		switch (forma)
		{
			case Forma_terreno.RampaSubida: return altura;
			case Forma_terreno.RampaBajada: return -altura;
			default: return 0f;
		}
	}

	private static float Altura(Forma_terreno forma, float t, float altura)
	{
		float suave = t * t * (3f - 2f * t); // 0 a 1 sin quiebre en los extremos
		float bulto = 0.5f * (1f - Mathf.Cos(2f * Mathf.PI * t)); // 0 a 1 y de vuelta a 0

		switch (forma)
		{
			case Forma_terreno.RampaSubida: return altura * suave;
			case Forma_terreno.RampaBajada: return -altura * suave;
			case Forma_terreno.Loma: return altura * bulto;
			case Forma_terreno.Valle: return -altura * bulto;
			default: return 0f;
		}
	}
}
