using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIAnimationManager : MonoBehaviour
{
	public RectTransform uiButton1;
	public RectTransform uiButton2;
	public RectTransform uiButton3;
	public RectTransform uiButton4;
	public TextMeshProUGUI headerText;
	public TextMeshProUGUI welcomeUserText;

	private CanvasGroup canvasGroup1;
	private CanvasGroup canvasGroup2;
	private CanvasGroup canvasGroup3;
	private CanvasGroup canvasGroup4;
	private CanvasGroup headerCanvasGroup;
	private CanvasGroup welcomeUserCanvasCroup;

	void Start()
	{
		// Butonlar�n CanvasGroup bile�enlerini otomatik olarak al�yoruz
		canvasGroup1 = uiButton1.GetComponent<CanvasGroup>();
		canvasGroup2 = uiButton2.GetComponent<CanvasGroup>();
		canvasGroup3 = uiButton3.GetComponent<CanvasGroup>();
		canvasGroup4 = uiButton4.GetComponent<CanvasGroup>();

		// Header i�in CanvasGroup ekle
		headerCanvasGroup = headerText.gameObject.GetComponent<CanvasGroup>();
		if (headerCanvasGroup == null)
		{
			headerCanvasGroup = headerText.gameObject.AddComponent<CanvasGroup>();
		}

		welcomeUserCanvasCroup = welcomeUserText.GetComponent<CanvasGroup>();
		if (welcomeUserCanvasCroup == null)
		{
			welcomeUserCanvasCroup = welcomeUserText.gameObject.AddComponent<CanvasGroup>();
		}

		// Ba�lang��ta butonlar� ve header'� tamamen transparan yap
		canvasGroup1.alpha = 0;
		canvasGroup2.alpha = 0;
		canvasGroup3.alpha = 0;
		canvasGroup4.alpha = 0;
		headerCanvasGroup.alpha = 0;
		welcomeUserCanvasCroup.alpha = 0;

		// Ba�lang��ta -200px sola al (header ve butonlar i�in)
		uiButton1.anchoredPosition -= new Vector2(200f, 0);
		uiButton2.anchoredPosition -= new Vector2(200f, 0);
		uiButton3.anchoredPosition -= new Vector2(200f, 0);
		uiButton4.anchoredPosition -= new Vector2(200f, 0);
		headerText.rectTransform.anchoredPosition -= new Vector2(200f, 0); // Header i�in sola kayd�rma
		welcomeUserText.rectTransform.anchoredPosition += new Vector2(200f, 0);

		// 1 saniye bekledikten sonra Header animasyonu ba�las�n
		float headerDelay = 1f;
		headerCanvasGroup.DOFade(1, 3f).SetDelay(headerDelay);
		headerText.rectTransform.DOAnchorPosX(headerText.rectTransform.anchoredPosition.x + 200f, 2f).SetDelay(headerDelay);

		// 2 saniye bekledikten sonra UI Butonlar�n�n animasyonu ba�las�n
		float buttonDelay = 2f;
		canvasGroup1.DOFade(1, 3f).SetDelay(buttonDelay);
		canvasGroup2.DOFade(1, 3f).SetDelay(buttonDelay);
		canvasGroup3.DOFade(1, 3f).SetDelay(buttonDelay);
		canvasGroup4.DOFade(1, 3f).SetDelay(buttonDelay);
		welcomeUserCanvasCroup.DOFade(1, 3f).SetDelay(buttonDelay);

		// Butonlar� 200 piksel sa�a kayd�r (ayn� anda hareket etmeye ba�larlar)
		uiButton1.DOAnchorPosX(uiButton1.anchoredPosition.x + 200f, 2f).SetDelay(buttonDelay);
		uiButton2.DOAnchorPosX(uiButton2.anchoredPosition.x + 200f, 2f).SetDelay(buttonDelay);
		uiButton3.DOAnchorPosX(uiButton3.anchoredPosition.x + 200f, 2f).SetDelay(buttonDelay);
		uiButton4.DOAnchorPosX(uiButton4.anchoredPosition.x + 200f, 2f).SetDelay(buttonDelay);
		welcomeUserText.rectTransform.DOAnchorPosX(welcomeUserText.rectTransform.anchoredPosition.x - 200f, 2f).SetDelay(buttonDelay);
	}
}
