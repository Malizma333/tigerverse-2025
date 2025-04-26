using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class ImagePull : MonoBehaviour
{
    public string imageUrl = "https://tigerverse-2025.onrender.com/image/latest";
    public Renderer targetImage; // Or use Image if you prefer UI Sprite

    void Start()
    {
        StartCoroutine(DownloadImage());
    }

    IEnumerator DownloadImage()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            targetImage.material.mainTexture = texture;

            float width = texture.width;
            float height = texture.height;
            float aspectRatio = width / height;

            transform.localScale = new Vector3(aspectRatio, 1f, 1f);
        }

    }
}