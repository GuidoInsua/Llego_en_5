using NUnit.Framework;
using UnityEngine;

public class Perfil_terrenoTests
{
	[TestCase(Forma_terreno.Plano)]
	[TestCase(Forma_terreno.RampaSubida)]
	[TestCase(Forma_terreno.RampaBajada)]
	[TestCase(Forma_terreno.Loma)]
	[TestCase(Forma_terreno.Valle)]
	public void EmpiezaEnCeroYTerminaEnElDesnivelFinal(Forma_terreno forma)
	{
		var puntos = Perfil_terreno.Generar(forma, 10f, 2f, 0.5f);

		Assert.AreEqual(Vector2.zero, puntos[0]);
		Assert.AreEqual(10f, puntos[puntos.Count - 1].x, 0.001f);
		Assert.AreEqual(Perfil_terreno.DesnivelFinal(forma, 2f), puntos[puntos.Count - 1].y, 0.001f);
	}

	[Test]
	public void LaLomaSubeLaAlturaPedida()
	{
		var puntos = Perfil_terreno.Generar(Forma_terreno.Loma, 10f, 2f, 0.5f);

		Assert.AreEqual(2f, puntos[puntos.Count / 2].y, 0.001f);
	}
}
