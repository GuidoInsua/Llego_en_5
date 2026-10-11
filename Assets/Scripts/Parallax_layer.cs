using UnityEngine;
using UnityEngine.Rendering;

public class Parallax_layer : MonoBehaviour
{
	[Header("Capa")]
	[SerializeField] private Transform camara;
	// 1 = queda fija en pantalla (cielo), 0 = se mueve junto al mundo (edificios cercanos)
	[SerializeField, Range(0f, 1f)] private float seguirCamara = 0.5f;

	[Header("Copias")]
	[SerializeField] private Transform[] copias; // Otras 2 copias de la misma tira (Ctrl+D)

	private float ancho;
	private float offsetY; // Altura de la capa respecto de la cámara, para que no se mueva al saltar

	void Start()
	{
		// Ancho de la tira en unidades del mundo, para repetirla sin cortes
		ancho = GetComponent<SpriteRenderer>().bounds.size.x;
		offsetY = transform.position.y - camara.position.y;
	}

	void OnEnable() => RenderPipelineManager.beginCameraRendering += Reposicionar;
	void OnDisable() => RenderPipelineManager.beginCameraRendering -= Reposicionar;

	// Justo antes de dibujar y no en LateUpdate: si corría antes que Cinemachine, leía la cámara del frame anterior y la capa temblaba
	private void Reposicionar(ScriptableRenderContext contexto, Camera cam)
	{
		if (cam.transform != camara) return;

		float x = camara.position.x;

		// Cuánto se corre el patrón de la tira respecto de la cámara
		float corrimiento = Mathf.Repeat(x * (1f - seguirCamara), ancho);
		float inicioX = x - corrimiento - ancho;

		// Tres tiras seguidas cubren toda la vista mientras el ancho sea mayor o igual a la mitad de la vista
		Posicionar(transform, inicioX);
		for (int i = 0; i < copias.Length; i++)
		{
			Posicionar(copias[i], inicioX + ancho * (i + 1));
		}
	}

	private void Posicionar(Transform tira, float posX)
	{
		// Y sigue a la cámara: el fondo no sube ni baja al saltar
		Vector3 pos = new Vector3(posX, camara.position.y + offsetY, tira.position.z);
		tira.position = pos;
	}
}
