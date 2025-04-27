using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class DefaultImage : MonoBehaviour
{
    public Renderer target;
    private string imageUrl = "https://tigerverse-2025.onrender.com/image/latest";

    private void Start()
    {
        StartCoroutine(DownloadImage());
    }

    private IEnumerator DownloadImage()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download image");
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);

        target.material.mainTexture = texture;

        MaintainAspectRatio(texture);
    }

    private void MaintainAspectRatio(Texture2D texture)
    {
        float width = texture.width;
        float height = texture.height;
        float aspectRatio = width / height;

        Vector3 scale = target.transform.localScale;
        scale.x = aspectRatio; 
        scale.y = 1f;          
        scale.z = 1f;        

        target.transform.localScale = scale;
    }
}
