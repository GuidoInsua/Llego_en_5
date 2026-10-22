using UnityEngine;
using UnityEngine.EventSystems;

// Al mostrarse un menú deja marcada su primera opción, para manejarlo con flechas y Enter
public class Seleccion_inicial : MonoBehaviour
{
	[Tooltip("La opción que queda marcada al abrir este menú.")]
	[SerializeField] private GameObject primeraOpcion;

	void Awake()
	{
		if (primeraOpcion == null)
		{
			Debug.LogError("Seleccion_inicial: falta asignar la primera opción.");
			enabled = false;
		}
	}

	void OnEnable()
	{
		Seleccionar();
	}

	// Al cargar la escena el EventSystem puede prenderse después que este menú
	void Start()
	{
		Seleccionar();
	}

	// Si un clic con el mouse dejó todo sin marcar, las flechas vuelven a funcionar
	void Update()
	{
		if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
		{
			Seleccionar();
		}
	}

	private void Seleccionar()
	{
		if (enabled && EventSystem.current != null)
		{
			EventSystem.current.SetSelectedGameObject(primeraOpcion);
		}
	}
}
