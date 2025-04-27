using UnityEngine;

public class ScaleManager : MonoBehaviour
{
    public Vector3 baseTransform;

    // Minimum and maximum values for the slider (scale range)
    public float minScale = 0.25f;
    public float maxScale = 16f;
    public float curScale = 1f;

    void Awake() {
        baseTransform = transform.localScale;
    }

    public void ReassignAspectRatio(Vector3 newBase) {
        baseTransform = newBase;
    }

    // Event function to change scale based on the slider value
    public void UpdateScale(float value)
    {
        // Maintain the aspect ratio by setting the same scale for all axes
        curScale = Mathf.Lerp(minScale, maxScale, value);

        // Update the object's scale, preserving the aspect ratio
        transform.localScale = baseTransform * curScale;
    }
}
