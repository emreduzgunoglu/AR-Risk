using UnityEngine;

public class SpinnerRotator : MonoBehaviour
{
    public float rotationSpeed = 200f; // saniyede dönecek derece

    void Update()
    {
        transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
    }
}
