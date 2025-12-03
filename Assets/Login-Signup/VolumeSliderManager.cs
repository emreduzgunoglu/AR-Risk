using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeSliderManager : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumeText;
    [SerializeField] private AudioSource musicAudioSource;


    private void Awake()
    {
        if (!PlayerPrefs.HasKey("musicVolume"))
            PlayerPrefs.SetFloat("musicVolume", 1f);
    }

    private void Start()
    {
        Load();
        ApplyVolume(); // oyun başlarken sesi uygula
        volumeSlider.onValueChanged.AddListener(delegate { ChangeVolume(); }); // dinleyici ekle
    }

    private void OnEnable()
    {
        Load(); // panel açıldığında slider'ı güncelle
        ApplyVolume(); // ses seviyesini uygula
    }

    public void ChangeVolume()
    {
        ApplyVolume(); // hem sesi hem text'i güncelle
        Save();
    }

    private void ApplyVolume()
    {
        musicAudioSource.volume = volumeSlider.value;

        volumeText.text = Mathf.RoundToInt(volumeSlider.value * 100) + "%";
    }

    private void Load()
    {
        float savedVolume = PlayerPrefs.GetFloat("musicVolume");
        volumeSlider.value = savedVolume;
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("musicVolume", volumeSlider.value);
    }
}