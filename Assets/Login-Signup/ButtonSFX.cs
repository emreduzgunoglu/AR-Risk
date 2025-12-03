using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource sfxSource;

    void Start()
    {
       

        sfxSource = GameObject.Find("SFX_AudioSource")?.GetComponent<AudioSource>();

        if (sfxSource == null)
        {
           
        }

        if (clickSound == null)
        {
            
        }

        GetComponent<Button>().onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
       
        sfxSource.volume = PlayerPrefs.GetFloat("sfxVolume", 1f);
        sfxSource.PlayOneShot(clickSound);
    }
}
