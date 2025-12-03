using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class DragToMove : MonoBehaviour
{
    private ARRaycastManager raycastManager;
    private Camera arCamera;
    private bool isDragging = false;

    static List<ARRaycastHit> hits = new();

    void Start()
    {
        raycastManager = FindFirstObjectByType<ARRaycastManager>();
        arCamera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = arCamera.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
                {
                    isDragging = true;
                }
            }

            if (touch.phase == TouchPhase.Moved && isDragging)
            {
                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    transform.position = hitPose.position;
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                isDragging = false;
            }
        }

        // İki parmakla döndürme (opsiyonel)
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0), t1 = Input.GetTouch(1);
            Vector2 prev0 = t0.position - t0.deltaPosition;
            Vector2 prev1 = t1.position - t1.deltaPosition;

            float prevAngle = Vector2.SignedAngle(prev1 - prev0, Vector2.right);
            float currAngle = Vector2.SignedAngle(t1.position - t0.position, Vector2.right);
            float delta = currAngle - prevAngle;

            transform.Rotate(0, -delta, 0);
        }
    }
}
