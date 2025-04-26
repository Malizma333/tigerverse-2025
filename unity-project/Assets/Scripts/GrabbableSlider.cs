using UnityEngine;
using UnityEngine.Events;

public class GrabbableSlider : MonoBehaviour
{
    public Transform thumb;
    public float minX = -0.1f;
    public float maxX = 0.1f;
    public UnityEvent<float> onValueChanged;

    private void Update()
    {
        float value = Mathf.InverseLerp(minX, maxX, thumb.localPosition.x);
        onValueChanged.Invoke(value);
    }
}
