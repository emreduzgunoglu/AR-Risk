using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CharacterSelector : MonoBehaviour
{
    public Image characterImage;                // Ortadaki karakter görseli
    public List<Sprite> characterSprites;       // Tüm karakter görselleri
    private int currentIndex = 0;

    public float fadeDuration = 0.3f;           // Smooth geçiş süresi

    private void Start()
    {
        // Kayıtlı index varsa onu yükle
        currentIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        currentIndex = Mathf.Clamp(currentIndex, 0, characterSprites.Count - 1);
        UpdateCharacterImageImmediate();
    }

    public void ShowPreviousCharacter()
    {
        if (characterSprites.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = characterSprites.Count - 1;

        PlayerPrefs.SetInt("SelectedCharacterIndex", currentIndex);
        StartCoroutine(SmoothCharacterChange(characterSprites[currentIndex]));
    }

    public void ShowNextCharacter()
    {
        if (characterSprites.Count == 0) return;

        currentIndex++;
        if (currentIndex >= characterSprites.Count)
            currentIndex = 0;

        PlayerPrefs.SetInt("SelectedCharacterIndex", currentIndex);
        StartCoroutine(SmoothCharacterChange(characterSprites[currentIndex]));
    }

    private void UpdateCharacterImageImmediate()
    {
        characterImage.sprite = characterSprites[currentIndex];
    }

    private IEnumerator SmoothCharacterChange(Sprite newSprite)
    {
        // Mevcut görseli şeffaf yap
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            SetImageAlpha(alpha);
            yield return null;
        }

        characterImage.sprite = newSprite;

        // Yeni sprite'ı görünür yap
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            SetImageAlpha(alpha);
            yield return null;
        }

        SetImageAlpha(1f);
    }

    private void SetImageAlpha(float alpha)
    {
        var color = characterImage.color;
        color.a = alpha;
        characterImage.color = color;
    }
}
