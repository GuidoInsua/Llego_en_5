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
// Desc: Probabilidades de las excusas según el tiempo de carrera y si convencen al jefe (sin Unity, se puede probar).

using UnityEngine;

// Cuentas puras: el valor al azar (0 a 1) lo pasa quien llama
public static class Sorteo_excusas
{
	private const float Tercio = 1f / 3f;

	// Con el tiempo estándar las tres categorías salen con 1/3. Cada segundo por debajo pasa ajustePorSegundo de mala a buena
	// (por encima, de buena a mala); la intermedia queda en 1/3
	public static (float buena, float mala) Probabilidades(float tiempo, float tiempoEstandar, float ajustePorSegundo)
	{
		float corrimiento = Mathf.Clamp((tiempoEstandar - tiempo) * ajustePorSegundo, -Tercio, Tercio);
		return (Tercio + corrimiento, Tercio - corrimiento);
	}

	public static Categoria_excusa SortearCategoria(float tiempo, float tiempoEstandar, float ajustePorSegundo, float azar)
	{
		(float buena, float mala) = Probabilidades(tiempo, tiempoEstandar, ajustePorSegundo);
		if (azar < buena) return Categoria_excusa.Buena;
		if (azar < buena + mala) return Categoria_excusa.Mala;
		return Categoria_excusa.Intermedia;
	}

	// La buena siempre convence, la mala nunca y la intermedia con probabilidadIntermedia
	public static bool Convence(Categoria_excusa categoria, float probabilidadIntermedia, float azar)
	{
		return categoria == Categoria_excusa.Buena || (categoria == Categoria_excusa.Intermedia && azar < probabilidadIntermedia);
	}
}
