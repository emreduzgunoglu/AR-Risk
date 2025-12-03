using UnityEngine;
using UnityEngine.InputSystem;

public class CubeSelection : MonoBehaviour
{
	public Material selectedMaterial;
	private Material originalMaterial;
	private Renderer objectRenderer;
	private Vector3 originalScale;

	// Se?ili olan objeyi takip etmek i?in static bir referans
	private static CubeSelection currentlySelectedObject;

	private void Start()
	{
		objectRenderer = GetComponent<Renderer>();
		if (objectRenderer != null)
		{
			originalMaterial = objectRenderer.material;
		}

		originalScale = transform.localScale;
	}

	private void Update()
	{
		// Dokunma (touch) veya t?klama (mouse) alg?lama
		if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
		{
			HandleTouchOrClick(Touchscreen.current.primaryTouch.position.ReadValue());
		}
		else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
		{
			HandleTouchOrClick(Mouse.current.position.ReadValue());
		}
	}

	private void HandleTouchOrClick(Vector2 screenPosition)
	{
		Ray ray = Camera.main.ScreenPointToRay(screenPosition);
		if (Physics.Raycast(ray, out RaycastHit hit))
		{
			if (hit.transform == transform)
			{
				SelectObject();
			}
		}
	}

	private void SelectObject()
	{
		// E?er ba?ka bir obje se?ilmi?se, onu eski haline d?nd?r
		if (currentlySelectedObject != null && currentlySelectedObject != this)
		{
			currentlySelectedObject.DeselectObject();
		}

		// Bu objeyi se?ili olarak i?aretle
		currentlySelectedObject = this;

		// Renk ve boyut de?i?tirme
		if (objectRenderer != null && selectedMaterial != null)
		{
			objectRenderer.material = selectedMaterial;
			transform.localScale = originalScale + new Vector3(0, 0, 0.8f);
		}
	}

	private void DeselectObject()
	{
		// Objeyi eski haline d?nd?rme
		if (objectRenderer != null)
		{
			objectRenderer.material = originalMaterial;
			transform.localScale = originalScale;
		}
	}
}