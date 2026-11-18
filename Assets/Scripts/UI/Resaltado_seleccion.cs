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

	void Update()
	{
		// Lo que está marcado ahora (puede ser nada)
		GameObject actual = null;
		if (EventSystem.current != null)
		{
			actual = EventSystem.current.currentSelectedGameObject;
		}

		// Solo trabaja cuando cambia la selección
		if (actual == seleccionado)
		{
			return;
		}

		// Le apaga el contorno al que estaba marcado antes
		if (contorno != null)
		{
			contorno.enabled = false;
		}
		seleccionado = actual;
		contorno = null;

		if (actual == null)
		{
			return;
		}

		Selectable control = actual.GetComponent<Selectable>();
		if (control == null || control.targetGraphic == null)
		{
			return;
		}

		// La primera vez que se marca este control se le agrega el contorno; las siguientes se reutiliza
		contorno = control.targetGraphic.GetComponent<Outline>();
		if (contorno == null)
		{
			contorno = control.targetGraphic.gameObject.AddComponent<Outline>();
			contorno.effectColor = colorContorno;
			contorno.effectDistance = grosor;
		}
		contorno.enabled = true;
	}
}
