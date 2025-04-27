using UnityEngine;

public class ScaleChanger : MonoBehaviour
{
    // Minimum and maximum values for the slider (scale range)
    public float minScale = 0.1f;
    public float maxScale = 5f;

    private Vector3 initialScale;

    void Start()
    {
        // Store the initial scale of the object
        initialScale = transform.localScale;
    }

    // Event function to change scale based on the slider value
    public void UpdateScale(float value)
    {
        // Maintain the aspect ratio by setting the same scale for all axes
        float scaleFactor = Mathf.Lerp(minScale, maxScale, value);

        // Update the object's scale, preserving the aspect ratio
        transform.localScale = initialScale * scaleFactor;
    }
}
