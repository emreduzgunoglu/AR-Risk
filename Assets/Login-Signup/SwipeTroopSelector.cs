using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class SwipeTroopSelector : MonoBehaviour
{
    public TMP_Text leftText, centerText, rightText;
    public Button confirmButton;
    public int current = 3;
    public int min = 1, max = 10;
    private Vector2 startTouchPos;
    public float threshold = 50f;

    void Start() => UpdateTexts();

    void Update()
    {
        // MOBIL: Parmak swipe
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
                startTouchPos = t.position;
            else if (t.phase == TouchPhase.Ended)
            {
                float delta = t.position.x - startTouchPos.x;
                if (Mathf.Abs(delta) > threshold)
                {
                    if (delta > 0) current = (current < max) ? current + 1 : min;
                    else if (delta < 0) current = (current > min) ? current - 1 : max;
                    Handheld.Vibrate();
                    AnimateText();
                    UpdateTexts();
                }
            }
        }

#if UNITY_EDITOR
        // EDITOR: Fare sürükleme
        if (Input.GetMouseButtonDown(0))
            startTouchPos = Input.mousePosition;

        if (Input.GetMouseButtonUp(0))
        {
            float delta = Input.mousePosition.x - startTouchPos.x;
            if (Mathf.Abs(delta) > threshold)
            {
                if (delta > 0) current = (current < max) ? current + 1 : min;
                else if (delta < 0) current = (current > min) ? current - 1 : max;
                AnimateText();
                UpdateTexts();
            }
        }
#endif
    }

    void UpdateTexts()
    {
        centerText.text = current.ToString();
        leftText.text = (current > min) ? (current - 1).ToString() : max.ToString();
        rightText.text = (current < max) ? (current + 1).ToString() : min.ToString();
    }

    void AnimateText()
    {
        centerText.transform.DOScale(1.2f, 0.1f).SetEase(Ease.OutQuad).OnComplete(() =>
            centerText.transform.DOScale(1f, 0.1f).SetEase(Ease.InQuad));
    }

    public void OnConfirm()
    {
        Debug.Log("Seçilen asker sayısı: " + current);
        // Buraya Firebase'e yazma veya transferi tetikleme gelebilir
    }
}
