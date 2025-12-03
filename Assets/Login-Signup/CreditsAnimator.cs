using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class TMPTextCreditsAnimator : MonoBehaviour
{
    [System.Serializable]
    public struct CreditEntry
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI roleText;
    }

    public CreditEntry[] credits;
    public float delayBetween = 0.5f;

     void OnEnable()
    {
        // Panel her açıldığında önceki tüm animasyonları sıfırla
        KillAllAnimations();
        HideAllCredits();
        StartCoroutine(AnimateCredits());
    }

    void KillAllAnimations()
    {
        foreach (var entry in credits)
        {
            if (entry.nameText != null)
            {
                DOTween.Kill(entry.nameText);
                DOTween.Kill(entry.nameText.transform);
            }

            if (entry.roleText != null)
            {
                DOTween.Kill(entry.roleText);
                DOTween.Kill(entry.roleText.transform);
            }
        }
    }

    void HideAllCredits()
    {
        foreach (var entry in credits)
        {
            if (entry.nameText != null)
            {
                entry.nameText.alpha = 0f;
                entry.nameText.transform.localScale = Vector3.zero;
            }

            if (entry.roleText != null)
            {
                entry.roleText.alpha = 0f;
                entry.roleText.transform.localScale = Vector3.zero;
            }
        }
    }

    IEnumerator AnimateCredits()
    {
        foreach (var entry in credits)
        {
            entry.nameText.DOFade(1f, 0.5f);
            entry.roleText.DOFade(1f, 0.5f);

            entry.nameText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
            entry.roleText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(delayBetween);
        }
    }
    void OnDisable()
{
    KillAllAnimations();
    HideAllCredits();
}

}
