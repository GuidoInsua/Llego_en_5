using System.Reflection;
using NUnit.Framework;

// Ayudas compartidas por los tests
public static class Test_utils
{
	// Asigna un campo [SerializeField] private; falla si el campo se renombró
	public static void SetField(object target, string name, object value)
	{
		FieldInfo campo = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.IsNotNull(campo, $"{target.GetType().Name} no tiene el campo '{name}'");
		campo.SetValue(target, value);
	}
}
