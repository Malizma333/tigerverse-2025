using UnityEngine;

public class ScaleManager : MonoBehaviour
{
    public Vector3 baseTransform;

    // Minimum and maximum values for the slider (scale range)
    public float minScale = 0.25f;
    public float maxScale = 4f;
    public float curScale = 1f;

    void Start() {
        baseTransform = transform.localScale;
    }

    void Update() {
        Debug.Log(baseTransform);
        Debug.Log(transform.localScale);
        if (!(baseTransform.x == transform.localScale.x / transform.localScale.z && baseTransform.y == transform.localScale.y / transform.localScale.z)) {
            baseTransform = transform.localScale / transform.localScale.z;
        }
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
