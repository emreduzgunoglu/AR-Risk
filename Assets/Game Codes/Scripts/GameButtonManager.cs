using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;

public class GameButtonManager : MonoBehaviour
{
	public GameManager gameManager;

	private void Start()
	{
		if (gameManager == null)
		{
			gameManager = FindAnyObjectByType<GameManager>();
		}
	}

	private void Update()
	{
		if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
		{
			Vector2 pos = Touchscreen.current.primaryTouch.position.ReadValue();
			_ = HandleTouchOrClickAsync(pos); 
		}
		else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
		{
			Vector2 pos = Mouse.current.position.ReadValue();
			_ = HandleTouchOrClickAsync(pos); 
		}
	}

	private async Task HandleTouchOrClickAsync(Vector2 screenPosition)
	{
		Ray ray = Camera.main.ScreenPointToRay(screenPosition);
		if (Physics.Raycast(ray, out RaycastHit hit))
		{
			if (hit.transform == transform)
			{
				await OnButtonClickAsync();
			}
		}
	}

	private async Task OnButtonClickAsync()
	{
		if (gameManager != null)
		{
			await gameManager.NextPhaseButtonAsync(); // doğru şekilde await edildi
		}
		else
		{
			Debug.LogError("GameManager bulunamadı!");
		}
	}
}
