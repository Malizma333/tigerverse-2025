using UnityEngine;

public class SnapHorizontally : MonoBehaviour
{
    public void OnValueChange(bool isOn)
    {
        Vector3 rotation = this.transform.eulerAngles;
        rotation.x = 90f;
        this.transform.eulerAngles = rotation;
    }
}
