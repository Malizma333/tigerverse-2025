using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class ImagePull : MonoBehaviour
{
    private string imageListUrl = "https://tigerverse-2025.onrender.com/images";
    private string imageFetchUrlBase = "https://tigerverse-2025.onrender.com/image/";
    public GameObject prefabToCopy;
    public Renderer imagePanel;

    void Start()
    {
        StartCoroutine(DownloadIDsAndImages());
    }

    IEnumerator DownloadIDsAndImages()
    {
        UnityWebRequest request = UnityWebRequest.Get(imageListUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load IDs: " + request.error);
            yield break;
        }

        string jsonResult = request.downloadHandler.text;
        List<ImageData> images = ParseJsonArray(jsonResult);

        if (images == null || images.Count == 0)
        {
            Debug.LogError("No images found in the server response.");
            yield break;
        }

        foreach (ImageData imageData in images)
        {
            StartCoroutine(DownloadImageAndCreatePrefab(imageData));
        }
    }

    IEnumerator DownloadImageAndCreatePrefab(ImageData imageData)
    {
        string imageUrl = imageFetchUrlBase + imageData.original;

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load image for ID {imageData.original}: {request.error}");
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);

        GameObject newInstance = Instantiate(prefabToCopy, this.transform);
        ShowImage showImage = newInstance.GetComponentInChildren<ShowImage>();

        showImage.target = imagePanel;

        // Store the image data (VERY important step!)
        ImageInfoHolder holder = newInstance.GetComponent<ImageInfoHolder>();
        if (holder != null)
        {
            holder.imageData = imageData;
        }
        else
        {
            Debug.LogError("Prefab is missing ImageInfoHolder script!");
        }

        Transform backgroundTransform = newInstance.transform.Find("Content/Background");

        if (backgroundTransform != null)
        {
            Image targetImage = backgroundTransform.GetComponent<Image>();
            RectTransform backgroundRectTransform = backgroundTransform.GetComponent<RectTransform>();

            if (targetImage != null && backgroundRectTransform != null)
            {
                Sprite downloadedSprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                targetImage.sprite = downloadedSprite;
                targetImage.preserveAspect = true;
                backgroundRectTransform.sizeDelta = new Vector2(200, 200);
                backgroundRectTransform.localScale = Vector3.one;
            }
            else
            {
                Debug.LogError("No Image or RectTransform on Background of prefab instance.");
            }
        }
        else
        {
            Debug.LogError("No Content/Background found inside prefab instance.");
        }
    }

    List<ImageData> ParseJsonArray(string json)
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
