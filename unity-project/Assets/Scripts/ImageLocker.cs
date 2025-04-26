using UnityEngine;

public class ImageLocker : MonoBehaviour
{
    private bool isLocked = false;
    private Vector3 lockedPosition;

    public void OnPinchLock()
    {
        isLocked = !isLocked;
        if (isLocked)
        {
            lockedPosition = transform.position;
        }
    }

    private void Update()
    {
        if (isLocked)
        {
            transform.position = lockedPosition; // Freeze position
        }
    }
}
