using UnityEngine;

public class ShowLayers : MonoBehaviour
{
    public void OnChangeValue(bool isOn)
    {
        if (isOn)
        {
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
}
