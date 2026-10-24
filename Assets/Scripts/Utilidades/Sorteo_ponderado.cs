// Sorteo con pesos: recibe el valor al azar como parámetro, así se puede probar sin Unity
public static class Sorteo_ponderado
{
	// valor01 en [0, 1). Devuelve el índice elegido; un peso 0 nunca sale. -1 si todos los pesos son 0
	public static int Elegir(float[] pesos, float valor01)
	{
		float total = 0f;
		foreach (float peso in pesos) total += peso;
		if (total <= 0f) return -1;

		float objetivo = valor01 * total;
		int ultimo = -1;
		for (int i = 0; i < pesos.Length; i++)
		{
			if (pesos[i] <= 0f) continue;

			ultimo = i;
			objetivo -= pesos[i];
			if (objetivo < 0f) return i;
		}

		return ultimo; // valor01 = 1 o error de redondeo
	}
}
