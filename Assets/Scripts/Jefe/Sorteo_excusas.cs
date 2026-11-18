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

// Solo cuentas: el número al azar (de 0 a 1) lo pasa quien llama, así se puede probar con valores fijos
public static class Sorteo_excusas
{
	private const float Tercio = 1f / 3f;

	// Con el tiempo estándar las tres categorías salen con 1/3. Cada segundo por debajo pasa ajustePorSegundo de mala a buena
	// (por encima, de buena a mala); la intermedia queda siempre en 1/3
	public static Categoria_excusa SortearCategoria(float tiempo, float tiempoEstandar, float ajustePorSegundo, float azar)
	{
		// Positivo si llegó antes del estándar, negativo si llegó después
		float segundosDeDiferencia = tiempoEstandar - tiempo;

		// Lo que pasa de mala a buena. Como mucho 1/3 para cada lado, así ninguna probabilidad queda negativa
		float corrimiento = Mathf.Clamp(segundosDeDiferencia * ajustePorSegundo, -Tercio, Tercio);

		float probabilidadBuena = Tercio + corrimiento;
		float probabilidadMala = Tercio - corrimiento;

		// El número al azar cae en uno de tres tramos: [buena][mala][intermedia]
		if (azar < probabilidadBuena)
		{
			return Categoria_excusa.Buena;
		}
		if (azar < probabilidadBuena + probabilidadMala)
		{
			return Categoria_excusa.Mala;
		}
		return Categoria_excusa.Intermedia;
	}

	// La buena siempre convence, la mala nunca y la intermedia con probabilidadIntermedia
	public static bool Convence(Categoria_excusa categoria, float probabilidadIntermedia, float azar)
	{
		if (categoria == Categoria_excusa.Buena)
		{
			return true;
		}
		if (categoria == Categoria_excusa.Intermedia)
		{
			return azar < probabilidadIntermedia;
		}
		return false;
	}
}
