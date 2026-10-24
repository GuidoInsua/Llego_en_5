using NUnit.Framework;

public class Sorteo_ponderadoTests
{
	[Test]
	public void ElijeSegunElPesoYSaltaLosCeros()
	{
		float[] pesos = { 1f, 0f, 3f };
		Assert.AreEqual(0, Sorteo_ponderado.Elegir(pesos, 0.1f));
		Assert.AreEqual(2, Sorteo_ponderado.Elegir(pesos, 0.3f));
		Assert.AreEqual(2, Sorteo_ponderado.Elegir(pesos, 1f));
		Assert.AreEqual(-1, Sorteo_ponderado.Elegir(new float[] { 0f, 0f }, 0.5f));
	}
}
