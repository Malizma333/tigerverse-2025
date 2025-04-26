using UnityEngine;

public class LockImage : MonoBehaviour
{
    public GameObject img;

    public void OnValueChanged(bool isOn)
    {
        if (img != null)
        {
            MonoBehaviour[] allScripts = img.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (MonoBehaviour script in allScripts)
            {
                if (script != this)
                {
                    script.enabled = !isOn;
                }
            }
        }
    }
}
