using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;
using System.Globalization;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;

public class UIPlayerManager : MonoBehaviour
{
    public static UIPlayerManager Instance;
    private DatabaseReference databaseReference; 
    public UImanager uimanager;
    
    public string playerOneName; // Kullanıcı Adı

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Sahne geçişlerinde objeyi koru
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        }
        else
        {
            Destroy(gameObject);  // Başka bir kopya varsa yok et
        }
    }

    public void Start()
    {
       //databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Start butonuna basıldığında çağrılacak
    public void StartGame()
	{
        string username = FirebaseAuth.DefaultInstance.CurrentUser?.DisplayName;
        if (string.IsNullOrEmpty(username))
        {
            Debug.Log("Kullanıcı adı yok!: " + username);
            return;
        }
        playerOneName = FormatName(username);
        string status = "Active";
        string timestamp = DateTime.Now.ToString("HH:mm:ss");

        PlayerData.Instance.SetUserData(playerOneName, status, timestamp); 

        DatabaseReference roomRef = FirebaseDatabase.DefaultInstance.RootReference.Child("Games").Child("Room1");
        roomRef.Child("Action").Child("GameStarted").GetValueAsync().ContinueWithOnMainThread(async task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("GameStarted kontrol edilemedi.");
                return;
            }

            bool gameAlreadyStarted = task.Result.Exists && Convert.ToBoolean(task.Result.Value);
            if (!gameAlreadyStarted)
            {
                Debug.Log("Oyun başlatılıyor, harita dağıtılıyor...");

                // 1. GameStarted, Phase, Turn yaz
                await roomRef.Child("Action").Child("GameStarted").SetValueAsync(true);
                await roomRef.Child("Phase").SetValueAsync(1);
                await roomRef.Child("Turn").SetValueAsync(1);
                await roomRef.Child("Action").Child("First").SetValueAsync("-");
                await roomRef.Child("Action").Child("Second").SetValueAsync("-");
                await roomRef.Child("Action").Child("PhaseOneAskerCount").SetValueAsync(0);
                await roomRef.Child("Action").Child("WinRate").SetValueAsync(0);

                // 2. Haritayı dağıt
                await AssignMapRandomly(roomRef.Child("Map"));

                // 3. Oyuncu bilgisi ekle ve sahneye geç
                UpdatePlayerCredentials(playerOneName, status, timestamp);
            }
            else
            {
                Debug.Log("Oyun daha önce başlatılmış. Oyuncu ekleniyor...");
                UpdatePlayerCredentials(playerOneName, status, timestamp);
            }
        });
	}

    private void UpdatePlayerCredentials(string playerName, string status, string timestamp)
    {
        // playerName altında bir oyuncu oluşturuluyor, status ise bu oyuncunun alt etiketi olarak ekleniyor
        var playerData = new Dictionary<string, object>
        {
            { "Status", status },  // Status bilgisi playerName altında ekleniyor
            { "Date", timestamp}
        };

        // Firebase'deki oyuncu adı (playerName) ve altındaki status bilgisi güncelleniyor
        databaseReference.Child("Games").Child("Room1").Child("Players").Child(playerName).UpdateChildrenAsync(playerData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"{playerName} - Status set to -> {status}");
                    // GameScene'e geçiş yapılıyor
                    SceneManager.LoadScene("GameScene");
                }
                else
                {
                    Debug.LogError($"{playerName} adı ve durumu güncellenirken hata oluştu: {task.Exception}");
                }
            });
    }

    public void QuitRoomOne()
    {
        string username = FirebaseAuth.DefaultInstance.CurrentUser?.DisplayName;
        if (string.IsNullOrEmpty(username))
        {
            Debug.Log("Kullanıcı adı yok!: " + username);
            return;
        }

        string activeUserName = FormatName(username); 

        Dictionary<string, object> updates = new()
        {
            { "Status", "Passive" }
        };

        databaseReference
            .Child("Games")
            .Child("Room1")
            .Child("Players")
            .Child(activeUserName)
            .UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log($"{activeUserName} - Status set to -> Passive");
                    if (uimanager != null)
                    {
                        uimanager.QuitRoomOnePressed();
                    }
                }
                else
                {
                    Debug.LogError($"{activeUserName} adı güncellenirken hata oluştu: {task.Exception}");
                }
            });
    }

    public void JoinRoomOne()
    {
        string username = FirebaseAuth.DefaultInstance.CurrentUser?.DisplayName;
        if (string.IsNullOrEmpty(username))
        {
            Debug.Log("Kullanıcı adı yok!: " + username);
            return;
        }
        playerOneName = FormatName(username);
        Debug.Log("USERNAME: " + playerOneName);
        string status = "Active";
        string timestamp = DateTime.Now.ToString("HH:mm:ss");

        PlayerData.Instance.SetUserData(playerOneName, status, timestamp);

        // Firebase'deki oyuncu adı güncelleniyor
        JoinRoomOneStatusUpdate(playerOneName, status, timestamp);
    }

    public void JoinRoomOneStatusUpdate(string playerName, string status, string timestamp)
    {
        // playerName altında bir oyuncu oluşturuluyor, status ise bu oyuncunun alt etiketi olarak ekleniyor
        var playerData = new Dictionary<string, object>
        {
            { "Status", status },  // Status bilgisi playerName altında ekleniyor
            { "Date", timestamp}
        };

        // Firebase'deki oyuncu adı (playerName) ve altındaki status bilgisi güncelleniyor
        databaseReference.Child("Games").Child("Room1").Child("Players").Child(playerName).UpdateChildrenAsync(playerData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"{playerName} - Status set to -> {status}");
                    if (uimanager != null)
                    {
                        uimanager.JoinRoomOnePressed();
                    }
                }
                else
                {
                    Debug.LogError($"{playerName} adı ve durumu güncellenirken hata oluştu: {task.Exception}");
                }
            });
    }

    // Kullanıcı adını düzgün formatta almak için
    private string FormatName(string name)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
    }

    // Oyuncu adını almak
    public string GetPlayerName(int index)
    {
        return index switch
        {
            0 => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(playerOneName.ToLower()),
            _ => "Unknown",
        };
    }

    private async Task AssignMapRandomly(DatabaseReference mapRef)
    {
        var allRegions = new List<string>(MapUtils.Neighbors.Keys);
        string[] players = { "Player1", "Player2", "Player3" };
        int playerCount = players.Length;
        int regionCount = allRegions.Count;

        System.Random rng = new();

        // 1. Bölgeleri karıştır ve sırayla dağıt (eşit sayıda)
        allRegions = allRegions.OrderBy(_ => rng.Next()).ToList();
        Dictionary<string, string> regionOwners = new(); // region -> player

        for (int i = 0; i < allRegions.Count; i++)
        {
            string player = players[i % playerCount];
            regionOwners[allRegions[i]] = player;
        }

        // 2. Her oyuncuya eşit toplam asker (örneğin toplam 180 → 60’ar)
        int totalTroops = regionCount * 3; // ortalama 3 asker
        int baseTroopsPerPlayer = totalTroops / playerCount;
        int remainder = totalTroops % playerCount;

        Dictionary<string, int> playerTroopPool = new();
        for (int i = 0; i < playerCount; i++)
        {
            playerTroopPool[players[i]] = baseTroopsPerPlayer + (i < remainder ? 1 : 0);
        }

        // 3. Her oyuncunun bölgelerine rastgele asker dağıt
        Dictionary<string, object> updates = new();
        Dictionary<string, List<string>> playerRegions = new();

        foreach (var region in regionOwners)
        {
            if (!playerRegions.ContainsKey(region.Value))
                playerRegions[region.Value] = new List<string>();

            playerRegions[region.Value].Add(region.Key);
        }

        foreach (var player in players)
        {
            List<string> regions = playerRegions[player];
            int troopPool = playerTroopPool[player];
            int regionsLeft = regions.Count;

            foreach (var region in regions)
            {
                int remainingMinTroops = regionsLeft; // Her bölgede en az 1 olacak
                int maxForThis = troopPool - (remainingMinTroops - 1);
                int assigned = rng.Next(1, Math.Min(7, maxForThis + 1));

                updates[$"{region}/Owner"] = player;
                updates[$"{region}/Troops"] = assigned;

                troopPool -= assigned;
                regionsLeft--;
            }
        }

        // 4. Firebase’e tek seferde gönder
        await mapRef.UpdateChildrenAsync(updates);

        Debug.Log("Tüm bölgeler eşit şekilde rastgele dağıtıldı.");
    }

}
