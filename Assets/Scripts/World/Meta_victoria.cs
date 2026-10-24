using System.Collections;
using UnityEngine;

// Pasado el tiempo pone una caja verde delante del jugador; al tocarla se muestra la pantalla de victoria
// Es para tener algo que probar: la meta real (el jefe) todavía no existe
public class Meta_victoria : MonoBehaviour
{
	[SerializeField] private Transform playerTransform;
	[SerializeField] private Caja_victoria cajaPrefab; // Pivote abajo: se apoya sobre el suelo
	[SerializeField] private GameObject pantallaVictoria; // Empieza apagada
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private float segundosParaAparecer = 60f;
	[SerializeField] private float distanciaDelante = 14f; // Menor que el suelo ya generado (2 tramos por delante)

	void Awake()
	{
		if (playerTransform == null || cajaPrefab == null || pantallaVictoria == null)
		{
			Debug.LogError("Meta_victoria: falta playerTransform, cajaPrefab o pantallaVictoria.");
			enabled = false;
		}
	}

	void Start()
	{
		StartCoroutine(EsperarYCrearCaja());
	}

	private IEnumerator EsperarYCrearCaja()
	{
		yield return new WaitForSeconds(segundosParaAparecer); // Respeta la pausa

		// Si adelante hay un hueco (desnivel) no hay suelo donde apoyarla: reintenta
		RaycastHit2D suelo;
		while (!(suelo = Physics2D.Raycast(new Vector2(playerTransform.position.x + distanciaDelante, playerTransform.position.y + 10f), Vector2.down, 40f, groundLayer)))
		{
			yield return null;
		}

		Instantiate(cajaPrefab, suelo.point, Quaternion.identity).Iniciar(Ganar);
	}

	private void Ganar()
	{
		pantallaVictoria.SetActive(true);
		Time.timeScale = 0f;
		AudioListener.pause = true;
	}
}
