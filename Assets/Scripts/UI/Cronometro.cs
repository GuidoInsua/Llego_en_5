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
// Desc: Tiempo de la partida en el HUD: suma segundos con cada golpe y hace un efecto al golpearse o pisar un puente.

using System.Collections;
using TMPro;
using UnityEngine;

// Tiempo de la partida, arriba a la derecha. Respeta la pausa (usa Time.deltaTime)
public class Cronometro : MonoBehaviour
{
	[SerializeField] private TMP_Text texto;
	[SerializeField] private Player_controller jugador;
	[Tooltip("Segundos que se suman al tiempo con cada golpe contra un obstáculo.")]
	[SerializeField] private float segundosPorGolpe = 5f;

	[Header("Pulso al golpearse o pisar un puente")]
	[SerializeField] private float duracionPulso = 0.6f;
	[SerializeField] private float escalaPulso = 1.3f;
	[SerializeField] private Color colorPulso = Color.red;
	[SerializeField] private float intervaloTitileo = 0.1f;

	private float segundos;
	private bool isDetenido;
	private Coroutine pulso;
	private Color colorNormal;
	private int decimasMostradas = -1; // Las décimas que muestra el texto ahora

	public float Segundos
	{
		get { return segundos; }
	}

	void Awake()
	{
		if (texto == null || jugador == null)
		{
			Debug.LogError("Cronometro: falta asignar el texto o el jugador.");
			enabled = false;
			return;
		}

		colorNormal = texto.color;
		jugador.Golpeado += SumarGolpe;
		jugador.Ralentizado += Pulsar;
	}

	void OnDestroy()
	{
		if (jugador != null)
		{
			jugador.Golpeado -= SumarGolpe;
			jugador.Ralentizado -= Pulsar;
		}
	}

	void Update()
	{
		if (isDetenido)
		{
			return;
		}

		segundos += Time.deltaTime;

		// El texto cambia 10 veces por segundo: solo se rearma cuando cambia la décima
		int decimas = (int)(segundos * 10f);
		if (decimas != decimasMostradas)
		{
			decimasMostradas = decimas;
			texto.text = Formatear(segundos);
		}
	}

	public void Detener()
	{
		isDetenido = true;
	}

	// Escribe el tiempo como mm:ss,d (minutos, segundos y décimas)
	public static string Formatear(float segundos)
	{
		// Se pasa a un número entero de décimas: así 59,96 s no se redondea a "60,0"
		int decimasTotales = (int)(segundos * 10f);

		int minutos = decimasTotales / 600; // 600 décimas = 1 minuto
		int decimasDelMinuto = decimasTotales % 600;
		int segundosDelMinuto = decimasDelMinuto / 10;
		int decima = decimasTotales % 10;

		return minutos.ToString("00") + ":" + segundosDelMinuto.ToString("00") + "," + decima;
	}

	// Frenar solo no castiga lo suficiente: el golpe también suma tiempo
	private void SumarGolpe()
	{
		segundos += segundosPorGolpe;
		Pulsar();
	}

	// Agranda y achica el texto y lo titila en rojo un ratito
	private void Pulsar()
	{
		if (pulso != null)
		{
			StopCoroutine(pulso);
		}
		pulso = StartCoroutine(Pulso());
	}

	private IEnumerator Pulso()
	{
		float tiempo = 0f;
		while (tiempo < duracionPulso)
		{
			// El seno de 0 a PI va de 0 a 1 y vuelve a 0: el texto crece y se achica
			float avance = tiempo / duracionPulso;
			float curva = Mathf.Sin(avance * Mathf.PI);
			float escala = 1f + (escalaPulso - 1f) * curva;
			texto.rectTransform.localScale = Vector3.one * escala;

			// Cambia de color cada intervaloTitileo segundos: un intervalo rojo, uno normal
			int numeroDeIntervalo = (int)(tiempo / intervaloTitileo);
			if (numeroDeIntervalo % 2 == 0)
			{
				texto.color = colorPulso;
			}
			else
			{
				texto.color = colorNormal;
			}

			yield return null;
			tiempo += Time.deltaTime;
		}

		texto.rectTransform.localScale = Vector3.one;
		texto.color = colorNormal;
		pulso = null;
	}
}
