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

// Va en el TMP del texto. Pone el texto completo y va mostrando más letras con maxVisibleCharacters:
// así las letras no cambian de lugar mientras se escribe
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

		float tiempo = 0f;
		while (texto.maxVisibleCharacters < frase.Length)
		{
			// Primero espera un cuadro: la tecla que cerró la frase anterior no tiene que saltear esta
			yield return null;

			if (Controles.SaltarTextoPresionado)
			{
				// Espacio: muestra la frase entera
				texto.maxVisibleCharacters = frase.Length;
			}
			else
			{
				int letras = (int)(tiempo / segundosPorLetra);
				if (letras > frase.Length)
				{
					letras = frase.Length;
				}
				texto.maxVisibleCharacters = letras;
			}

			tiempo += Time.deltaTime;
		}
	}
}
