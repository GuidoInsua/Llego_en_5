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
// Desc: Contorno de luz blanca sutil en el botón o slider marcado con el teclado.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Va en el EventSystem. Marca lo seleccionado con un contorno blanco; Enter lo aprieta (lo hace el módulo de input)
public class Resaltado_seleccion : MonoBehaviour
{
	[SerializeField] private Color colorContorno = new Color(1f, 1f, 1f, 0.45f);
	[SerializeField] private Vector2 grosor = new Vector2(4f, -4f);

	private GameObject seleccionado;
	private Outline contorno;

	// Solo trabaja cuando cambia la selección
	void Update()
	{
		EventSystem sistema = EventSystem.current;
		GameObject actual = sistema != null ? sistema.currentSelectedGameObject : null;
		if (actual == seleccionado) return;

		if (contorno != null) contorno.enabled = false;
		seleccionado = actual;
		contorno = null;

		if (actual != null && actual.TryGetComponent(out Selectable control) && control.targetGraphic != null)
		{
			if (!control.targetGraphic.TryGetComponent(out contorno))
			{
				contorno = control.targetGraphic.gameObject.AddComponent<Outline>();
				contorno.effectColor = colorContorno;
				contorno.effectDistance = grosor;
			}
			contorno.enabled = true;
		}
	}
}
