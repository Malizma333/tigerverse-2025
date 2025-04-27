using UnityEngine;

public class CloseDevConsoleScript : MonoBehaviour
{
    void Start()
    {
        Debug.developerConsoleVisible = false;
    }
}