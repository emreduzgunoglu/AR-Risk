using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using DG.Tweening;

public class PlacementManager : MonoBehaviour
{
    public GameObject placementIndicatorPrefab;
    public GameObject finalMapPrefab;
    public GameObject mapPlacementContainer;
    public GameObject phoneImage;
    public GameObject MapPlacementOkeyContainer;
    public GameObject[] buttonsToReveal;

    private GameObject currentIndicator;
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private bool isPlaced = false;

    private Tween phoneImageTween;
    static List<ARRaycastHit> hits = new();

    void Start()
    {
        raycastManager = FindFirstObjectByType<ARRaycastManager>();
        planeManager = FindFirstObjectByType<ARPlaneManager>();

        if (planeManager != null)
        {
            planeManager.trackablesChanged.AddListener(OnPlanesChanged);
        }

        currentIndicator = Instantiate(placementIndicatorPrefab);
        currentIndicator.SetActive(false);

        StartCoroutine(WaitUntilSceneIsFullyReady());
    }

    private IEnumerator WaitUntilSceneIsFullyReady()
    {
        // Kamera hazır mı? Frame başladı mı? 0.1 saniye bile olsa beklet
        yield return new WaitUntil(() => Camera.main != null);
        yield return new WaitForEndOfFrame();

        // Alternatif olarak aşağıdakini de ekleyebilirsin:
        // yield return new WaitUntil(() => Time.timeSinceLevelLoad > 0.2f);

        yield return new WaitForSeconds(0.5f); // Ekstra güvenlik

        StartCoroutine(ShowPlacementGuideSequence()); // şimdi göster
    }

    void Update()
    {
        if (isPlaced) return;

        Vector2 center = new(Screen.width / 2, Screen.height / 2);
        if (raycastManager.Raycast(center, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;

            Vector3 normal = pose.rotation * Vector3.up;
            float dot = Vector3.Dot(normal, Vector3.up);
            if (dot > 0.9f)
            {
                Vector3 adjustedPosition = pose.position + new Vector3(0f, 0.015f, 0f);
                currentIndicator.SetActive(true);
                currentIndicator.transform.SetPositionAndRotation(adjustedPosition, pose.rotation);
            }
        }

        if (currentIndicator != null)
        {
            Vector3 cameraPosition = Camera.main.transform.position;
            Vector3 lookAtPosition = new(cameraPosition.x, currentIndicator.transform.position.y, cameraPosition.z);
            currentIndicator.transform.LookAt(lookAtPosition);
        }
    }

    public void ConfirmPlacement()
    {
        if (isPlaced) return;
        isPlaced = true;

        Quaternion correctedRotation = currentIndicator.transform.rotation * Quaternion.Euler(0, 180, 0);
        Instantiate(finalMapPrefab, currentIndicator.transform.position, correctedRotation);
        Destroy(currentIndicator);

        if (planeManager != null)
            planeManager.enabled = false;

        foreach (var plane in FindObjectsByType<ARPlane>(FindObjectsSortMode.None))
        {
            plane.gameObject.SetActive(false);
        }

        if (MapPlacementOkeyContainer != null)
        {
            if (!MapPlacementOkeyContainer.TryGetComponent<CanvasGroup>(out var cg))
            {
                cg = MapPlacementOkeyContainer.AddComponent<CanvasGroup>();
            }

            cg.DOFade(0f, 1f).OnComplete(() =>
            {
                MapPlacementOkeyContainer.SetActive(false);
                ShowButtonsAfterPlacement();
            });
        }
    }

    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        foreach (var addedPlane in args.added)
        {
            if (addedPlane.alignment == PlaneAlignment.Vertical)
            {
                addedPlane.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator ShowPlacementGuideSequence()
    {
        yield return new WaitForSeconds(1f);

        if (mapPlacementContainer != null)
        {
            if (!mapPlacementContainer.TryGetComponent<CanvasGroup>(out var cg))
            {
                cg = mapPlacementContainer.AddComponent<CanvasGroup>();
            }

            mapPlacementContainer.SetActive(true);
            cg.alpha = 0f;
            cg.DOFade(1f, 1f);
        }

        if (phoneImage != null)
        {
            phoneImage.transform.localRotation = Quaternion.Euler(0, -50, 0);
            phoneImageTween = phoneImage.transform
                .DOLocalRotate(new Vector3(0, 50, 0), 1.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        yield return new WaitForSeconds(3f);

        if (mapPlacementContainer != null)
        {
            if (mapPlacementContainer.TryGetComponent<CanvasGroup>(out var cg))
            {
                cg.DOFade(0f, 1f).OnComplete(() =>
                {
                    mapPlacementContainer.SetActive(false);

                    if (MapPlacementOkeyContainer != null)
                    {
                        if (!MapPlacementOkeyContainer.TryGetComponent<CanvasGroup>(out var okeyCg))
                            okeyCg = MapPlacementOkeyContainer.AddComponent<CanvasGroup>();

                        MapPlacementOkeyContainer.SetActive(true);
                        okeyCg.alpha = 0f;
                        okeyCg.DOFade(1f, 1f);
                    }
                });
            }
            else
            {
                mapPlacementContainer.SetActive(false);

                if (MapPlacementOkeyContainer != null)
                {
                    if (!MapPlacementOkeyContainer.TryGetComponent<CanvasGroup>(out var okeyCg))
                        okeyCg = MapPlacementOkeyContainer.AddComponent<CanvasGroup>();

                    MapPlacementOkeyContainer.SetActive(true);
                    okeyCg.alpha = 0f;
                    okeyCg.DOFade(1f, 1f);
                }
            }
        }

        phoneImageTween?.Kill();
    }

    private void ShowButtonsAfterPlacement()
    {
        foreach (var button in buttonsToReveal)
        {
            if (button == null) continue;

            // Aktif etmeden önce alpha'yı sıfırla
            if (!button.TryGetComponent(out CanvasGroup cg))
            {
                cg = button.AddComponent<CanvasGroup>();
            }

            cg.alpha = 0f; // Görünmez yap
            button.SetActive(true); // Şimdi aktif yap (artık alpha 0)

            // Ardından fade-in başlat
            cg.DOFade(1f, 1f).SetEase(Ease.Linear);
        }
    }

}
