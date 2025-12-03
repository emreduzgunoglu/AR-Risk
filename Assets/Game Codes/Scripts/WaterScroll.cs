using System.Collections;
using UnityEngine;

public class WaterScroll : MonoBehaviour
{
	public Material waterMaterial; // Deniz materyalini buraya baðla
	public float scrollRange = 50f / 1024f; // UV space'de 50px hareket
	public float speed = 0.5f; // Hareket hýzý

	private float time;
	private float direction = 1f;

	void Update()
	{
		// Zaman ilerledikçe bir döngü oluþtur
		time += Time.deltaTime * speed * direction;
		float t = (Mathf.Sin(time) + 1) / 2; // 0 ile 1 arasýnda yavaþlayýp hýzlanan hareket
		float offsetX = Mathf.Lerp(-scrollRange, scrollRange, t);

		if (waterMaterial != null)
		{
			waterMaterial.mainTextureOffset = new Vector2(offsetX, waterMaterial.mainTextureOffset.y);
		}
	}
}
