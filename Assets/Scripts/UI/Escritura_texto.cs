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
// Desc: Escribe un texto letra por letra; la barra espaciadora lo completa de golpe.

using System.Collections;
using TMPro;
using UnityEngine;

// Va en el TMP del texto. Usa maxVisibleCharacters: el texto completo ya está puesto y no cambia de lugar al escribirse
[RequireComponent(typeof(TMP_Text))]
public class Escritura_texto : MonoBehaviour
{
	[SerializeField] private float segundosPorLetra = 0.03f;

	private TMP_Text texto;

	void Awake()
	{
		texto = GetComponent<TMP_Text>();
	}

	public IEnumerator Escribir(string frase)
	{
		texto.text = frase;
		texto.maxVisibleCharacters = 0;

		for (float t = 0f; texto.maxVisibleCharacters < frase.Length; t += Time.deltaTime)
		{
			// Primero espera un cuadro: la tecla que cerró la frase anterior no tiene que saltear esta
			yield return null;
			texto.maxVisibleCharacters = Controles.SaltarTextoPresionado ? frase.Length : Mathf.Min(frase.Length, (int)(t / segundosPorLetra));
		}
	}
}
