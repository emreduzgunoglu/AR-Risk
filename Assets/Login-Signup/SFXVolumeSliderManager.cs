using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SFXVolumeSliderManager : MonoBehaviour
{
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxVolumeText;
    [SerializeField] private AudioSource sfxAudioSource;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("sfxVolume"))
            PlayerPrefs.SetFloat("sfxVolume", 1f);
    }

    private void Start()
    {
        Load();
        ApplySFXVolume();
        sfxSlider.onValueChanged.AddListener(delegate { ChangeSFXVolume(); });
    }

    private void OnEnable()
    {
        Load();
        ApplySFXVolume();
    }

    public void ChangeSFXVolume()
    {
        ApplySFXVolume();
        Save();
    }

    private void ApplySFXVolume()
    {
        if (sfxAudioSource != null)
            sfxAudioSource.volume = sfxSlider.value;

        sfxVolumeText.text = Mathf.RoundToInt(sfxSlider.value * 100) + "%";
    }

    private void Load()
    {
        float savedVolume = PlayerPrefs.GetFloat("sfxVolume");
        sfxSlider.value = savedVolume;
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("sfxVolume", sfxSlider.value);
    }
}