using UnityEngine;
using TMPro;
using Firebase.Database;
using UnityEngine.UI;

public class RoomStatus : MonoBehaviour
{
    public TextMeshProUGUI roomOneStatusText;
    public TextMeshProUGUI roomOneStatusJoin;

    public TextMeshProUGUI PlayerOneTMP;
    public TextMeshProUGUI PlayerTwoTMP;
    public TextMeshProUGUI PlayerThreeTMP;

    public TextMeshProUGUI PlayerOneReady;
    public TextMeshProUGUI PlayerTwoReady;
    public TextMeshProUGUI PlayerThreeReady;

    public Button StartGameButton;

    private DatabaseReference reference;

    void Start()
    {
        if (Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            Debug.Log("Giriş yapılmadı, RoomStatus bekliyor...");
            return;
        }

        InitializeRoomStatusListener();
    }

    public void InitializeRoomStatusListener()
    {
        if (roomOneStatusText == null || roomOneStatusJoin == null || 
            PlayerOneTMP == null || PlayerTwoTMP == null || PlayerThreeTMP == null || StartGameButton == null)
        {
            Debug.LogError("Bazı TextMeshPro veya Button referansları eksik!");
            return;
        }

        reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("Games/Room1/Players").ValueChanged += HandlePlayersChanged;
    }

    private void HandlePlayersChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Veritabanı hatası: " + args.DatabaseError.Message);
            return;
        }

        if (this == null || !gameObject.activeInHierarchy) return;
        if (PlayerOneTMP == null || PlayerTwoTMP == null || PlayerThreeTMP == null || StartGameButton == null) return;


        int activePlayersCount = 0;

        // Temizle
        PlayerOneTMP.text = "Player Waiting...";
        PlayerTwoTMP.text = "Player Waiting...";
        PlayerThreeTMP.text = "Player Waiting...";

        PlayerOneReady.text = "<color=red>Not Ready</color>";
        PlayerTwoReady.text = "<color=red>Not Ready</color>";
        PlayerThreeReady.text = "<color=red>Not Ready</color>";

        int index = 0;

        foreach (var player in args.Snapshot.Children)
        {
            // Oyuncu adı = key
            string playerName = player.Key;

            if (player.HasChild("Status") && player.Child("Status").Value.ToString() == "Active")
            {
                activePlayersCount++;

                switch (index)
                {
                    case 0:
                        PlayerOneTMP.text = playerName;
                        PlayerOneReady.text = "<color=green>Ready</color>";
                        break;
                    case 1:
                        PlayerTwoTMP.text = playerName;
                        PlayerTwoReady.text = "<color=green>Ready</color>";
                        break;
                    case 2:
                        PlayerThreeTMP.text = playerName;
                        PlayerThreeReady.text = "<color=green>Ready</color>";
                        break;
                }
                index++;
            }
        }

        // Renkli durum yazısı
        string color = (activePlayersCount == 0) ? "red" : "green";
        if (roomOneStatusText != null)
            roomOneStatusText.text = $"<color={color}>{activePlayersCount}</color> / 3";

        if (roomOneStatusJoin != null)    
            roomOneStatusJoin.text = $"<color={color}>{activePlayersCount}</color> / 3";

        if (StartGameButton != null)
            StartGameButton.interactable = activePlayersCount == 3;
    }

    private void OnDestroy()
    {
        if (reference != null)
        {
            reference.Child("Games/Room1/Players").ValueChanged -= HandlePlayersChanged;
        }
    }
}
