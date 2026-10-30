using UnityEngine;

// Keeps a world-space sign (like a cage's price) turned towards the camera so it is always readable.
public class Billboard : MonoBehaviour
{
    private Transform cameraTransform;

    // LateUpdate runs after the camera has moved this frame
    void LateUpdate()
    {
        if (cameraTransform == null)
        {
            if (Camera.main == null) return;
            cameraTransform = Camera.main.transform;
        }
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
    }
}
