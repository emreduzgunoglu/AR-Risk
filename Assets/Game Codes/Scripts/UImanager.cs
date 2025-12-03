using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UImanager : MonoBehaviour
{
	public GameObject playIU;
	public GameObject exitUI;
	public GameObject settingsUI;
	public GameObject creditsUI;
	public GameObject gameRoomsUI;
	public GameObject LoginUI;
	public GameObject SignupUI;
	public GameObject RoomstatusUI;
	public GameObject ProfileRegisterButton;

	public LoginManager loginManager; 

	private void ClearAllAuthFields()
	{
		if (loginManager != null)
		{
			loginManager.ClearLoginAndRegisterFields();
		}
	}

	public void StartGameButton()
	{
		int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
		SceneManager.LoadScene(nextIndex);
	}
	public void ExitButton()
	{
		Application.Quit();
	}
	public void PlayButtonPress()
	{
		if (playIU.activeSelf)
		{
			playIU.SetActive(false);
			gameRoomsUI.SetActive(false);
		}
		else
		{
			playIU.SetActive(true);

			exitUI.SetActive(false);
			settingsUI.SetActive(false);
			creditsUI.SetActive(false);
			gameRoomsUI.SetActive(false);
			LoginUI.SetActive(false);
			SignupUI.SetActive(false);
			RoomstatusUI.SetActive(false);
			ClearAllAuthFields(); 
		}
	}

	public void ExitButtonPress()
	{
		if (exitUI.activeSelf)
		{
			exitUI.SetActive(false);
		}
		else
		{
			exitUI.SetActive(true);

			playIU.SetActive(false);
			settingsUI.SetActive(false);
			creditsUI.SetActive(false);
			gameRoomsUI.SetActive(false);
			LoginUI.SetActive(false);
			SignupUI.SetActive(false);
			RoomstatusUI.SetActive(false);
			ClearAllAuthFields(); 
		}
	}

	public void SettingsButtonPress()
	{
		if (settingsUI.activeSelf)
		{
			settingsUI.SetActive(false);
		}
		else
		{
			settingsUI.SetActive(true);

			playIU.SetActive(false);
			exitUI.SetActive(false);
			creditsUI.SetActive(false);
			gameRoomsUI.SetActive(false);
			LoginUI.SetActive(false);
			SignupUI.SetActive(false);
			RoomstatusUI.SetActive(false);
			ClearAllAuthFields(); 
		}
	}

	public void CreditsButtonPress()
	{
		if (creditsUI.activeSelf)
		{
			creditsUI.SetActive(false);
		}
		else
		{
			creditsUI.SetActive(true);

			playIU.SetActive(false);
			exitUI.SetActive(false);
			settingsUI.SetActive(false);
			gameRoomsUI.SetActive(false);
			LoginUI.SetActive(false);
			SignupUI.SetActive(false);
			RoomstatusUI.SetActive(false);
			ClearAllAuthFields(); 
		}
	}

	public void GameRoomsPress()
	{
		if (gameRoomsUI.activeSelf)
		{
			gameRoomsUI.SetActive(false);
		}
		else
		{
			if (FirebaseAuth.DefaultInstance.CurrentUser != null)
			{
				gameRoomsUI.SetActive(true);
			}
			else
			{
				playIU.SetActive(false);
				LoginUI.SetActive(true);
				ClearAllAuthFields(); 
			}
		}
	}

	public void RegisterbuttonPress()
	{
		if (LoginUI.activeSelf)
		{
			LoginUI.SetActive(false);
		}
		else
		{
			LoginUI.SetActive(true);

			playIU.SetActive(false);
			exitUI.SetActive(false);
			creditsUI.SetActive(false);
			gameRoomsUI.SetActive(false);
			settingsUI.SetActive(false);
			SignupUI.SetActive(false);
			ClearAllAuthFields(); 
		}
	}

	public void CreateNewAccountPressed()
	{
		if (SignupUI.activeSelf)
		{
			SignupUI.SetActive(false);
		}
		else
		{
			SignupUI.SetActive(true);

			playIU.SetActive(false);
			exitUI.SetActive(false);
			creditsUI.SetActive(false);
			gameRoomsUI.SetActive(false);
			settingsUI.SetActive(false);
			LoginUI.SetActive(false);
			ClearAllAuthFields(); 
		}
	}

	public void QuitRoomOnePressed()
	{
		RoomstatusUI.SetActive(false);

		playIU.SetActive(true);
		gameRoomsUI.SetActive(true);
	}

	public void JoinRoomOnePressed()
	{
		if (!RoomstatusUI.activeSelf) {
			RoomstatusUI.SetActive(true);

			playIU.SetActive(false);
			exitUI.SetActive(false);
			settingsUI.SetActive(false);
			creditsUI.SetActive(false);
			gameRoomsUI.SetActive(false);
			LoginUI.SetActive(false);
			SignupUI.SetActive(false);
			ClearAllAuthFields(); 
		}
	}

	public void ProfileRegisterButtonPressed()
	{
		if (LoginUI.activeSelf)
		{
			LoginUI.SetActive(false);
		}
		else
		{
			LoginUI.SetActive(true);

			playIU.SetActive(false);
			exitUI.SetActive(false);
			creditsUI.SetActive(false);
			gameRoomsUI.SetActive(false);
			settingsUI.SetActive(false);
			SignupUI.SetActive(false);
			ClearAllAuthFields(); 
		}
	}
			
}
