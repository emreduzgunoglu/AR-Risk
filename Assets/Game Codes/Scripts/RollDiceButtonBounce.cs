using UnityEngine;

public class RollDiceButtonBounce : MonoBehaviour
{
    public float bounceHeight = 10f;   // Zıplama yüksekliği (pixel)
    public float bounceSpeed = 2f;     // Zıplama hızı

    private RectTransform rectTransform;
    private Vector2 originalPosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // Sürekli yukarı-aşağı sinusoidal hareket
        float offsetY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        rectTransform.anchoredPosition = originalPosition + new Vector2(0, offsetY);
    }
}
