using System;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    private string playerName;
    private string status;
    private string timestamp;

    void Awake()
    {
        // Singleton yapısı
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Sahne geçişlerinde objeyi koru
        }
        else
        {
            Destroy(gameObject);  // Başka bir kopya varsa yok et
        }
    }

    // Kullanıcı verilerini kaydet
    public void SetUserData(string playerName, string status, string timestamp)
    {
        this.playerName = playerName;
        this.status = status;
        this.timestamp = timestamp; 
    }

    // Kullanıcı verilerini al
    public string GetPlayerName() => playerName;
    public string GetPlayerStatus() => status;
    public string GetTimestamp() => timestamp;
}
