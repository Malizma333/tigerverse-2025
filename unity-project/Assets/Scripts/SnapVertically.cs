using UnityEngine;

public class SnapVertically : MonoBehaviour
{
    public void OnValueChange(bool isOn)
    {
        Vector3 rotation = this.transform.eulerAngles;
        rotation.x = 0f;
        this.transform.eulerAngles = rotation;
    }
}
