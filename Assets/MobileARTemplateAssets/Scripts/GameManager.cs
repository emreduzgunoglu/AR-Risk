using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Firebase.Extensions;
using System.Threading.Tasks;
using UnityEngine.UI; // async-await desteği

public class GameManager : MonoBehaviour
{
	private DatabaseReference dbReference;
	public static GameManager Instance;

	// Player Credentials
	private string currentUserName;
	private string currentUserStatus;
	private string currentUserTime;

	public TextMeshProUGUI ScaleText;
	public static TextMeshPro TurnText;

	public static TextMeshPro PlayerOneText;
	public static TextMeshPro PlayerTwoText;
	public static TextMeshPro PlayerThreeText;

	public GameObject SettingsButton;
	private Coroutine scalingCoroutine;

	// Kıta +Bonuslar
	public static TextMeshPro SAContinentText;
	public static TextMeshPro NAContinentText;
	public static TextMeshPro AFContinentText;
	public static TextMeshPro EUContinentText;
	public static TextMeshPro ASContinentText;
	public static TextMeshPro AUContinentText;

	// Oyuncu bölge bilgileri
	public static TextMeshPro PlayerOneAskerText;
	public static TextMeshPro PlayerTwoAskerText;
	public static TextMeshPro PlayerThreeAskerText;

	// Sea 3D
	public GameObject SeaRendererButton;
	private Renderer seaRenderer;
	private Coroutine seaScaleCoroutine;
	private int seaScaleState = 0; 
	private Vector3 originalScale = new(2f, 1.7f, 1f);

	// 3D Objects
	private Renderer BaseRenderer;
	private Renderer PlayerOneBaseRenderer;
	private Renderer PlayerTwoBaseRenderer;
	private Renderer PlayerThreeBaseRenderer;

	private Renderer MoveStatusBase1Renderer;
	private Renderer MoveStatusBase2Renderer;
	private Renderer MoveStatusBase3Renderer;

	private Renderer NextPhaseButtonRenderer;
	public TextMeshPro NextPhaseButtonText;
	private bool isButtonLocked = false;

	// Orijinal materyal
	public Material originalBaseMaterial;

	// World Map Prefab
	private GameObject curvedWorldMap;
	private int scaleIndex = 1;

	private float searchTime = 0f;  // Geçen süreyi takip etmek için
	private readonly float maxSearchTime = 30f; // Maksimum 30 saniye boyunca arayacağız

	public Material redMaterial; // Kırmızı renk materyali

	void Start()
	{
		currentUserName = PlayerData.Instance.GetPlayerName();
		currentUserStatus = PlayerData.Instance.GetPlayerStatus();
		currentUserTime = PlayerData.Instance.GetTimestamp();
		SetPlayerActive();

		StartCoroutine(FindUIElementsCoroutine()); // Coroutine başlat
	}

	private IEnumerator FindUIElementsCoroutine()
	{
		while (searchTime < maxSearchTime)
		{
			GameObject turnTextObject = GameObject.Find("TurnTextPrefab");
			GameObject playerOneObject = GameObject.Find("PlayerOneTextPrefab");
			GameObject playerTwoObject = GameObject.Find("PlayerTwoTextPrefab");
			GameObject playerThreeObject = GameObject.Find("PlayerThreeTextPrefab");
			GameObject baseObject = GameObject.Find("Base");
			GameObject playerOneBaseObject = GameObject.Find("PlayerOneBasePrefab");
			GameObject playerTwoBaseObject = GameObject.Find("PlayerTwoBasePrefab");
			GameObject playerThreeBaseObject = GameObject.Find("PlayerThreeBasePrefab");
			GameObject curvedWorldMapObject = GameObject.Find("CurvedWorldMap");

			GameObject moveStatusBase1 = GameObject.Find("MoveStatusBase1");
			GameObject moveStatusBase2 = GameObject.Find("MoveStatusBase2");
			GameObject moveStatusBase3 = GameObject.Find("MoveStatusBase3");

			GameObject NextPhaseButton = GameObject.Find("NextPhaseButtonBase");
			GameObject NextPhaseButtonTextObject = GameObject.Find("NextPhaseButtonText");

			GameObject saText = GameObject.Find("SAContinentText");
			GameObject naText = GameObject.Find("NAContinentText");
			GameObject afText = GameObject.Find("AFContinentText");
			GameObject euText = GameObject.Find("EUContinentText");
			GameObject asText = GameObject.Find("ASContinentText");
			GameObject auText = GameObject.Find("AUContinentText");

			GameObject seaObj = GameObject.Find("SeaObject");

			GameObject playerOneAskerPrefab = GameObject.Find("PlayerOneAskerPrefab");
			GameObject playerTwoAskerPrefab = GameObject.Find("PlayerTwoAskerPrefab");
			GameObject playerThreeAskerPrefab = GameObject.Find("PlayerThreeAskerPrefab");

			if (turnTextObject != null && playerOneObject != null && playerTwoObject != null && playerThreeObject != null &&
				baseObject != null && playerOneBaseObject != null && playerTwoBaseObject != null && playerThreeBaseObject != null &&
				curvedWorldMapObject != null && moveStatusBase1 != null && moveStatusBase2 != null && moveStatusBase3 != null &&
				NextPhaseButton != null && NextPhaseButtonTextObject != null)
			{
				TurnText = turnTextObject.GetComponent<TextMeshPro>();
				PlayerOneText = playerOneObject.GetComponent<TextMeshPro>();
				PlayerTwoText = playerTwoObject.GetComponent<TextMeshPro>();
				PlayerThreeText = playerThreeObject.GetComponent<TextMeshPro>();

				BaseRenderer = baseObject.GetComponent<Renderer>();
				PlayerOneBaseRenderer = playerOneBaseObject.GetComponent<Renderer>();
				PlayerTwoBaseRenderer = playerTwoBaseObject.GetComponent<Renderer>();
				PlayerThreeBaseRenderer = playerThreeBaseObject.GetComponent<Renderer>();

				MoveStatusBase1Renderer = moveStatusBase1.GetComponent<Renderer>();
				MoveStatusBase2Renderer = moveStatusBase2.GetComponent<Renderer>();
				MoveStatusBase3Renderer = moveStatusBase3.GetComponent<Renderer>();

				NextPhaseButtonRenderer = NextPhaseButton.GetComponent<Renderer>();
				NextPhaseButtonText = NextPhaseButtonTextObject.GetComponent<TextMeshPro>();

				SAContinentText = saText.GetComponent<TextMeshPro>();
				NAContinentText = naText.GetComponent<TextMeshPro>();
				AFContinentText = afText.GetComponent<TextMeshPro>();
				EUContinentText = euText.GetComponent<TextMeshPro>();
				ASContinentText = asText.GetComponent<TextMeshPro>();
				AUContinentText = auText.GetComponent<TextMeshPro>();

				seaRenderer = seaObj.GetComponent<Renderer>();

				PlayerOneAskerText = playerOneAskerPrefab.GetComponent<TextMeshPro>();
				PlayerTwoAskerText = playerTwoAskerPrefab.GetComponent<TextMeshPro>();
				PlayerThreeAskerText = playerThreeAskerPrefab.GetComponent<TextMeshPro>();

				curvedWorldMap = curvedWorldMapObject;

				if (TurnText != null && PlayerOneText != null && PlayerTwoText != null && PlayerThreeText != null && BaseRenderer != null &&
					 PlayerOneBaseRenderer != null && PlayerTwoBaseRenderer != null && PlayerThreeBaseRenderer != null && curvedWorldMap != null &&
					 MoveStatusBase1Renderer != null && MoveStatusBase2Renderer != null && MoveStatusBase3Renderer != null && NextPhaseButtonRenderer != null)
				{
					originalBaseMaterial = BaseRenderer.material; // Orijinal materyali sakla
					ListenToPhaseChanges();
					ListenToTurnChanges();
					ListenStatusChanges();
					UpdatePlayerNames();
					UpdateTurnText();
					UpdateBaseMaterial();
					UpdateMoveStatusMaterial();
				}

				yield break; // Tüm nesneler bulundu, coroutine sonlandır
			}

			searchTime += 1f;
			yield return new WaitForSeconds(1f);
		}
	}

	// Oyundan çıkıldığında çalışır (Uygulama tamamen kapatılırsa)
	void OnApplicationQuit()
	{
		SetPlayerInactive();
	}

	// Oyun arka plana alındığında çalışır (Mobil cihazlarda minimize edildiğinde)
	/* void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SetPlayerInactive();
        }
    } */

	private async void SetPlayerInactive()
	{
		DatabaseReference playerStatusRef = FirebaseDatabase.DefaultInstance
			.GetReference("Games/Room1")
			.Child("Players").Child(currentUserName).Child("Status");

		await playerStatusRef.SetValueAsync("Passive");
		Debug.Log("- SET - " + currentUserName + " Pasife çekildi.");
	}

	private async void SetPlayerActive()
	{
		DatabaseReference playerStatusRef = FirebaseDatabase.DefaultInstance
			.GetReference("Games/Room1")
			.Child("Players").Child(currentUserName).Child("Status");

		await playerStatusRef.SetValueAsync("Active");

		Debug.Log("- SET - " + currentUserName + " Aktif hale getirildi.");
	}

	void ListenToPhaseChanges()
	{
		dbReference = FirebaseDatabase.DefaultInstance.GetReference("Games/Room1");
		DatabaseReference phaseRef = dbReference.Child("Phase");

		phaseRef.ValueChanged += (sender, args) =>
		{
			if (args.Snapshot.Exists)
			{
				int currentPhase = int.Parse(args.Snapshot.Value.ToString());
				Debug.Log("- GET - Phase: " + currentPhase);

				// UI sıfırla
				MoveStatusBase1Renderer.material = redMaterial;
				MoveStatusBase2Renderer.material = redMaterial;
				MoveStatusBase3Renderer.material = redMaterial;

				switch (currentPhase)
				{
					case 1:
						MoveStatusBase1Renderer.material = originalBaseMaterial;
						break;
					case 2:
						MoveStatusBase2Renderer.material = originalBaseMaterial;
						break;
					case 3:
						MoveStatusBase3Renderer.material = originalBaseMaterial;
						break;
				}

				UpdateTurnText();
				UpdateBaseMaterial();
			}
		};
	}

	public void ListenToTurnChanges()
	{
		dbReference = FirebaseDatabase.DefaultInstance.GetReference("Games/Room1");
		DatabaseReference turnRef = dbReference.Child("Turn");

		turnRef.ValueChanged += (sender, args) =>
	   {
		   UpdateTurnText();
		   UpdateBaseMaterial();
	   };
	}

	public void UpdateMoveStatusMaterial()
	{
		if (MoveStatusBase1Renderer != null) MoveStatusBase1Renderer.material = originalBaseMaterial;
		if (MoveStatusBase2Renderer != null) MoveStatusBase2Renderer.material = redMaterial;
		if (MoveStatusBase3Renderer != null) MoveStatusBase3Renderer.material = redMaterial;
	}

	public void BackButton()
	{
		SetPlayerInactive();
		int newIndex = SceneManager.GetActiveScene().buildIndex - 1;
		SceneManager.LoadScene(newIndex);
	}

	public void SettingsToggle()
	{
		if (SettingsButton.activeSelf) // SettingsButton'un aktif olup olmadığını kontrol eder
		{
			SettingsButton.SetActive(false); // Aktifse kapat
		}
		else
		{
			SettingsButton.SetActive(true); // Pasifse aç
		}
	}

	public void ExitButton()
	{
		Application.Quit();
	}

	// Firebase'deki oyuncu durumlarını dinle ve aktif olduğunda oyuncuları güncelle
	private void ListenStatusChanges()
	{
		FirebaseDatabase.DefaultInstance.RootReference
			.Child("Games")
			.Child("Room1")
			.Child("Players")
			.ChildChanged += (sender, args) =>
			{
				if (args.Snapshot.HasChild("Status") &&
					(args.Snapshot.Child("Status").Value.ToString() == "Active" ||
					 args.Snapshot.Child("Status").Value.ToString() == "Passive"))
				{
					UpdatePlayerNames();
				}
			};

		FirebaseDatabase.DefaultInstance.RootReference
			.Child("Games")
			.Child("Room1")
			.Child("Players")
			.ChildAdded += (sender, args) =>
			{
				// Yeni oyuncu eklendiğinde de aktif/pasif durumu kontrol et
				if (args.Snapshot.HasChild("Status") &&
					(args.Snapshot.Child("Status").Value.ToString() == "Active" ||
					args.Snapshot.Child("Status").Value.ToString() == "Passive"))
				{
					UpdatePlayerNames();
				}
			};
	}

	// Firebase'den aktif oyuncuları güncelle ve Text alanlarına atama yap
	private void UpdatePlayerNames()
	{
		FirebaseDatabase.DefaultInstance.RootReference
			.Child("Games")
			.Child("Room1")
			.Child("Players")
			.OrderByChild("Status")
			.EqualTo("Active")  // Sadece "Active" durumundaki oyuncuları al
			.LimitToFirst(3)  // Sadece 3 oyuncu
			.GetValueAsync()
			.ContinueWithOnMainThread(task =>
			{
				if (task.IsCompleted && task.Result.Exists)
				{
					var players = task.Result.Children;
					int playerCount = 0;

					PlayerOneText.text = "Waiting...";
					PlayerTwoText.text = "Waiting...";
					PlayerThreeText.text = "Waiting...";

					foreach (var player in players)
					{
						string playerName = player.Key;  // player adı)
						string status = player.Child("Status").Value.ToString();

						// Eğer oyuncunun durumu "Active" ise, playerCount ile uygun Text alanını güncelle
						if (status == "Active")
						{
							playerCount++;

							// Her oyuncuya göre Text alanlarını güncelle
							if (playerCount == 1)
							{
								PlayerOneText.text = playerName;
							}
							else if (playerCount == 2)
							{
								PlayerTwoText.text = playerName;
							}
							else if (playerCount == 3)
							{
								PlayerThreeText.text = playerName;
							}
						}
					}
				}
				else
				{
					PlayerOneText.text = "Waiting...";
					PlayerTwoText.text = "Waiting...";
					PlayerThreeText.text = "Waiting...";
					Debug.LogError("Aktif oyuncular Firebase’den alınamadı veya bulunamadı.");
				}
			});
	}

	// Turn yazısını güncelle
	private void UpdateTurnText()
	{
		if (TurnText == null)
		{
			Debug.LogWarning("TurnText değişkeni henüz atanmadı! Güncelleme yapılamadı.");
			return;
		}

		DatabaseReference turnRef = FirebaseDatabase.DefaultInstance
			.RootReference.Child("Games").Child("Room1").Child("Turn");

		turnRef.GetValueAsync().ContinueWithOnMainThread(task =>
		{
			if (task.IsCompleted && task.Result.Exists)
			{
				int turnValue = int.Parse(task.Result.Value.ToString()); // Turn değerini çek
				TurnText.text = GetPlayerName(turnValue) + "'s Turn";
				Debug.Log($" - GET - Turn değeri: {turnValue}");
			}
			else
			{
				Debug.LogError("Firebase'den Turn değeri okunamadı veya mevcut değil.");
			}
		});
	}

	private string GetPlayerName(int turn)
	{
		return turn switch
		{
			1 => PlayerOneText.text,
			2 => PlayerTwoText.text,
			3 => PlayerThreeText.text,
			_ => "hata",
		};
	}

	private void UpdateBaseMaterial()
	{
		if (BaseRenderer == null) return;

		dbReference = FirebaseDatabase.DefaultInstance.GetReference("Games/Room1");
		DatabaseReference turnRef = dbReference.Child("Turn");

		turnRef.GetValueAsync().ContinueWithOnMainThread(task =>
		{
			if (!task.IsCompleted || !task.Result.Exists)
			{
				Debug.LogError("Turn bilgisi alınamadı!");
				return;
			}

			int currentTurn = int.Parse(task.Result.Value.ToString());

			switch (currentTurn)
			{
				case 1:
					BaseRenderer.material = PlayerOneBaseRenderer.material;
					break;
				case 2:
					BaseRenderer.material = PlayerTwoBaseRenderer.material;
					break;
				case 3:
					BaseRenderer.material = PlayerThreeBaseRenderer.material;
					break;
				default:
					Debug.LogError("Geçersiz Turn değeri!");
					break;
			}
		});
	}

	public async Task NextPhaseButtonAsync()
	{
		if (isButtonLocked)
		{
			Debug.Log("Buton geçici olarak kilitli.");
			return;
		}

		Vector3 originalScale = new(0.5f, 0.2f, 1f);

		isButtonLocked = true;
		StartCoroutine(FadeButtonVisual(
			fromAlpha: 1f,
			toAlpha: 0f,
			fromScale: originalScale,
			toScale: originalScale * 0.8f,
			duration: 0.3f
		));

		_ = UnlockButtonAfterDelay();

		if (PlayerOneText.text == "Waiting..." || PlayerTwoText.text == "Waiting..." || PlayerThreeText.text == "Waiting...")
		{
			Debug.LogError("OYUNCULAR BEKLENİYOR...");
			return;
		}

		dbReference = FirebaseDatabase.DefaultInstance.GetReference("Games/Room1");
		DatabaseReference turnRef = dbReference.Child("Turn");
		DatabaseReference phaseRef = dbReference.Child("Phase");

		// Turn değerini al
		DataSnapshot turnSnapshot = await turnRef.GetValueAsync();
		if (!turnSnapshot.Exists)
		{
			Debug.LogError("Turn bilgisi alınamadı!");
			return;
		}

		int currentTurn = int.Parse(turnSnapshot.Value.ToString());
		string currentPlayer;

		// Turn değerine göre ilgili player text alınır
		switch (currentTurn)
		{
			case 1:
				currentPlayer = PlayerOneText.text;
				break;
			case 2:
				currentPlayer = PlayerTwoText.text;
				break;
			case 3:
				currentPlayer = PlayerThreeText.text;
				break;
			default:
				Debug.LogError("Geçersiz Turn değeri!");
				return;
		}

		// Kullanıcı sırası kontrol edilir
		if (currentPlayer != currentUserName)
		{
			Debug.Log("CurrentPlayer: " + currentPlayer + " | currentUserName: " + currentUserName);
			Debug.Log("Senin sıran değil");

			StartCoroutine(SmoothScaleInAndOut(BaseRenderer, 0.5f, 0.25f)); // 1.5f hedef ölçek, 1 saniye süresince yapılacak
			return;
		}

		// Phase işlemleri
		DataSnapshot phaseSnapshot = await phaseRef.GetValueAsync();
		if (phaseSnapshot.Exists)
		{
			int currentPhase = int.Parse(phaseSnapshot.Value.ToString());

			int nextPhase = (currentPhase % 3) + 1;
			if (currentPhase == 3 && nextPhase == 1)
			{
				await TurnIndexIncrease();
				UpdateTurnText();
				UpdateBaseMaterial();
			}

			await phaseRef.SetValueAsync(nextPhase);
			await dbReference.Child("Action").Child("First").SetValueAsync("-");
			await dbReference.Child("Action").Child("Second").SetValueAsync("-");
			await dbReference.Child("Action").Child("WinRate").SetValueAsync("0");
			await dbReference.Child("Action").Child("PhaseOneAskerCount").SetValueAsync("0");
			MapManager mapManager = FindFirstObjectByType<MapManager>();
			if (mapManager != null)
			{
				mapManager.ResetLocalSelections();
			}
			Debug.Log("- SET - Phase: " + nextPhase);
		}
		else
		{
			Debug.LogError("Phase bilgisi bulunamadı!");
		}
	}

	private async Task UnlockButtonAfterDelay()
	{
		await Task.Delay(2000);
		Vector3 originalScale = new(0.5f, 0.2f, 1f);

		// Butonu tekrar görünür ve normal boyut yap
		StartCoroutine(FadeButtonVisual(
			fromAlpha: 0f,
			toAlpha: 1f,
			fromScale: originalScale * 0.8f,
			toScale: originalScale,
			duration: 0.3f
		));

		isButtonLocked = false;
	}

	private IEnumerator FadeButtonVisual(float fromAlpha, float toAlpha, Vector3 fromScale, Vector3 toScale, float duration)
	{
		if (NextPhaseButtonRenderer == null) yield break;

		Material mat = NextPhaseButtonRenderer.material;

		// BURADA DEĞİŞİKLİK: Doğru renk kanalını alıyoruz
		Color baseColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") :
						mat.HasProperty("baseColor") ? mat.GetColor("baseColor") :
						mat.color;

		Transform buttonTransform = NextPhaseButtonRenderer.transform;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			float t = elapsed / duration;

			// Renk (alpha)
			float newAlpha = Mathf.Lerp(fromAlpha, toAlpha, t);
			Color newColor = new Color(baseColor.r, baseColor.g, baseColor.b, newAlpha);

			// BASECOLOR doğru olan hangisiyse ona set et
			if (mat.HasProperty("_BaseColor"))
				mat.SetColor("_BaseColor", newColor);
			else if (mat.HasProperty("baseColor"))
				mat.SetColor("baseColor", newColor);
			else
				mat.color = newColor;

			// Ölçek
			buttonTransform.localScale = Vector3.Lerp(fromScale, toScale, t);

			elapsed += Time.deltaTime;
			yield return null;
		}

		// Son değer sabitleniyor
		Color finalColor = new Color(baseColor.r, baseColor.g, baseColor.b, toAlpha);
		if (mat.HasProperty("_BaseColor"))
			mat.SetColor("_BaseColor", finalColor);
		else if (mat.HasProperty("baseColor"))
			mat.SetColor("baseColor", finalColor);
		else
			mat.color = finalColor;

		buttonTransform.localScale = toScale;
	}

	public async Task TurnIndexIncrease()
	{
		int currentTurn = await GetTurnValue(); // Mevcut Turn değerini al
		if (currentTurn == -1) return; // Hata varsa çık

		int newTurn = (currentTurn % 3) + 1; // 1 → 2 → 3 → tekrar 1

		await FirebaseDatabase.DefaultInstance.RootReference
			.Child("Games").Child("Room1").Child("Turn").SetValueAsync(newTurn); // Firebase’e yeni değeri kaydet
		Debug.Log($"Turn değeri güncellendi: {newTurn}");
	}

	// Firebase’den Turn değerini sırayla al
	private async Task<int> GetTurnValue()
	{
		DataSnapshot snapshot = await FirebaseDatabase.DefaultInstance.RootReference
			.Child("Games").Child("Room1").Child("Turn").GetValueAsync();
		if (snapshot.Exists)
		{
			return int.Parse(snapshot.Value.ToString()); // Turn değerini döndür
		}
		Debug.LogError("Turn değeri Firebase’den okunamadı.");
		return -1; // Hata durumunda
	}

	public void ScaleCurvedWorldMap()
	{
		// Eğer sahnede CurvedWorldMap bulunamadıysa koruma yapalım
		if (curvedWorldMap == null)
		{
			Debug.LogWarning("CurvedWorldMap sahnede bulunamad�!");
			return;
		}

		// Var olan bir Coroutine çalışıyorsa iptal et (örn. hızlı tıklama durumunda çakışmayı önlemek için)
		if (scalingCoroutine != null)
		{
			StopCoroutine(scalingCoroutine);
		}

		scaleIndex++;
		// 4'ü geçince başa dön
		if (scaleIndex > 4)
		{
			scaleIndex = 1;
		}

		ScaleText.text = "X" + scaleIndex;

		// Hedef �l�e�i hesapla
		float targetScale = 1f * scaleIndex;

		// Belirli bir s�re (�rn. 0.5 saniye) boyunca yava��a �l�eklemek i�in Coroutine ba�lat
		scalingCoroutine = StartCoroutine(SmoothScale(targetScale, 0.5f));
	}

	private IEnumerator SmoothScale(float targetScale, float duration)
	{
		// Mevcut �l�ek de�erini ba�lang�� noktas� olarak al
		float startScale = curvedWorldMap.transform.localScale.x;
		float elapsedTime = 0f;

		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / duration;

			// Lerp ile startScale'den targetScale'e lineer ge�i� yap
			float newScale = Mathf.Lerp(startScale, targetScale, t);
			curvedWorldMap.transform.localScale = new Vector3(newScale, newScale, newScale);

			yield return null;
		}

		// S�re dolunca tam hedef �l�e�e ayarla (float hatalar�n� �nlemek i�in)
		curvedWorldMap.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
	}

	private IEnumerator SmoothScaleInAndOut(Renderer targetRenderer, float targetScale, float duration)
	{
		// Mevcut ölçek değerini başlangıç noktası olarak al
		Vector3 startScale = targetRenderer.transform.localScale;

		for (int i = 0; i < 2; i++) // 2 kez tekrarlayacak döngü
		{
			// Ölçeği artır
			float elapsedTime = 0f;
			while (elapsedTime < duration)
			{
				elapsedTime += Time.deltaTime;
				float t = elapsedTime / duration;

				// Lerp ile startScale'den targetScale'e geçiş yap
				float newXScale = Mathf.Lerp(startScale.x, targetScale, t);
				targetRenderer.transform.localScale = new Vector3(newXScale, startScale.y, startScale.z);

				yield return null;
			}

			// Hedef ölçeğe ulaştıktan sonra, eski haline dön
			elapsedTime = 0f;
			while (elapsedTime < duration)
			{
				elapsedTime += Time.deltaTime;
				float t = elapsedTime / duration;

				// Lerp ile targetScale'den startScale'e geçiş yap
				float newXScale = Mathf.Lerp(targetScale, startScale.x, t);
				targetRenderer.transform.localScale = new Vector3(newXScale, startScale.y, startScale.z);

				yield return null;
			}
		}
	}

	public void ScaleSeaObjectSmooth()
	{
		if (seaRenderer == null)
		{
			Debug.LogWarning("SeaObject Renderer bulunamadı!");
			return;
		}

		if (seaScaleCoroutine != null)
		{
			StopCoroutine(seaScaleCoroutine);
		}

		// Yeni hedef scale değeri belirlenir
		Vector3 targetScale;

		if (seaScaleState == 0)
		{
			targetScale = new Vector3(0.01f, 0.01f, 0.01f); // Küçült
			seaScaleState = 1;
		}
		else
		{
			targetScale = originalScale; // Orijinal boyut
			seaScaleState = 0;
		}

		seaScaleCoroutine = StartCoroutine(SmoothScaleSea(seaRenderer.transform, targetScale, 0.5f));
	}

	private IEnumerator SmoothScaleSea(Transform targetTransform, Vector3 targetScale, float duration)
	{
		Vector3 startScale = targetTransform.localScale;
		float elapsedTime = 0f;

		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / duration;
			targetTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
			yield return null;
		}

		targetTransform.localScale = targetScale;
	}

}
