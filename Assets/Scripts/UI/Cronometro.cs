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
// Desc: Tiempo de la partida en el HUD, con efecto al recibir un golpe.

using System.Collections;
using TMPro;
using UnityEngine;

// Tiempo de la partida, arriba a la derecha. Respeta la pausa (usa Time.deltaTime)
public class Cronometro : MonoBehaviour
{
	[SerializeField] private TMP_Text texto;
	[SerializeField] private Player_controller jugador;

	[Header("Pulso al golpearse")]
	[SerializeField] private float duracionPulso = 0.6f;
	[SerializeField] private float escalaPulso = 1.3f;
	[SerializeField] private Color colorPulso = Color.red;
	[SerializeField] private float intervaloTitileo = 0.1f;

	private float segundos;
	private bool isDetenido;
	private Coroutine pulso;
	private Color colorNormal;

	public string Formato => Formatear(segundos);
	public float Segundos => segundos;

	void Awake()
	{
		if (texto == null || jugador == null)
		{
			Debug.LogError("Cronometro: falta asignar el texto o el jugador.");
			enabled = false;
			return;
		}

		colorNormal = texto.color;
		jugador.Golpeado += Pulsar;
	}

	void OnDestroy()
	{
		if (jugador != null) jugador.Golpeado -= Pulsar;
	}

	// Agranda y achica el texto y lo titila en rojo un ratito
	private void Pulsar()
	{
		if (pulso != null) StopCoroutine(pulso);
		pulso = StartCoroutine(Pulso());
	}

	private IEnumerator Pulso()
	{
		for (float t = 0f; t < duracionPulso; t += Time.deltaTime)
		{
			texto.rectTransform.localScale = Vector3.one * (1f + (escalaPulso - 1f) * Mathf.Sin(t / duracionPulso * Mathf.PI));
			texto.color = (int)(t / intervaloTitileo) % 2 == 0 ? colorPulso : colorNormal;
			yield return null;
		}

		texto.rectTransform.localScale = Vector3.one;
		texto.color = colorNormal;
		pulso = null;
	}

	void Update()
	{
		if (isDetenido) return;

		segundos += Time.deltaTime;
		texto.text = Formato;
	}

	public void Detener()
	{
		isDetenido = true;
	}

	// mm:ss,d
	public static string Formatear(float segundos)
	{
		int decimas = (int)(segundos * 10f); // Entero: así 59,96 s no se redondea a "60.0"
		return (decimas / 600).ToString("00") + ":" + (decimas % 600 / 10).ToString("00") + "," + decimas % 10;
	}
}
