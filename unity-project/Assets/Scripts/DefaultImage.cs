using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class ImageData
{
    public string original;
    public string detailed;
    public string textured;
}

public class DefaultImage : MonoBehaviour
{
    public Renderer original_r;
    public Renderer detailed_r;
    public Renderer textured_r;

    private string imageListUrl = "https://tigerverse-2025.onrender.com/images";
    private string imageFetchUrlBase = "https://tigerverse-2025.onrender.com/image/";

    private ImageInfoHolder infoHolder;

    private void Awake()
    {
        infoHolder = GetComponent<ImageInfoHolder>();
        if (infoHolder == null)
        {
            Debug.LogError("Missing ImageInfoHolder component!");
        }
    }

    private void Start()
    {
        StartCoroutine(DownloadLatestImage());
    }

    private IEnumerator DownloadLatestImage()
    {
        UnityWebRequest request = UnityWebRequest.Get(imageListUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download image list: " + request.error);
            yield break;
        }

        string jsonResult = request.downloadHandler.text;
        List<ImageData> images = ParseJsonArray(jsonResult);

        if (images == null || images.Count == 0)
        {
            Debug.LogError("No images found.");
            yield break;
        }

        ImageData latestImage = images[images.Count - 1];

        if (latestImage == null || string.IsNullOrEmpty(latestImage.original))
        {
            Debug.LogError("Latest image data missing original id.");
            yield break;
        }

        // Store the IDs in the ImageInfoHolder
        if (infoHolder != null)
        {
            infoHolder.imageData = latestImage;
        }

        // Download the original image
        string originalUrl = imageFetchUrlBase + latestImage.original;
        StartCoroutine(DownloadAndDisplayImage(originalUrl, original_r));

        string detailedUrl = imageFetchUrlBase + latestImage.detailed;
        StartCoroutine(DownloadAndDisplayImage(detailedUrl, detailed_r));
    
        string texturedUrl = imageFetchUrlBase + latestImage.textured;
        StartCoroutine(DownloadAndDisplayImage(texturedUrl, textured_r));
    }

    private IEnumerator DownloadAndDisplayImage(string url, Renderer rendererTarget)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download image: " + request.error);
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);

        if (texture == null)
        {
            Debug.LogError("Downloaded texture is null!");
            yield break;
        }
        else
        {
            Debug.Log($"Texture downloaded successfully from {url}. Size: {texture.width}x{texture.height}");
        }
        rendererTarget.material.mainTexture = texture;

        MaintainAspectRatio(texture, rendererTarget);
    }

    private void MaintainAspectRatio(Texture2D texture, Renderer rendererTarget)
    {
        float width = texture.width;
        float height = texture.height;
        float aspectRatio = width / height;

        Vector3 scale = rendererTarget.transform.localScale;
        scale.x = aspectRatio;
        scale.y = 1f;
        scale.z = 1f;

        rendererTarget.transform.localScale = scale;
    }

    private List<ImageData> ParseJsonArray(string json)
    {
        try
        {
            return JsonUtilityWrapper.FromJsonArray<ImageData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse JSON: " + e.Message);
            return null;
        }
    }
}

// Same helper:
public static class JsonUtilityWrapper
{
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
    }

    public static List<T> FromJsonArray<T>(string jsonArray)
    {
        string newJson = "{\"Items\":" + jsonArray + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.Items;
    }
}
