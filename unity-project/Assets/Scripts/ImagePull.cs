using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;


public class ImagePull : MonoBehaviour
{
    private string imageListUrl = "https://tigerverse-2025.onrender.com/images"; // This gives array of IDs
    private string imageFetchUrlBase = "https://tigerverse-2025.onrender.com/image/"; // Base for downloading by ID
    public GameObject prefabToCopy; // Drag your prefab here
    public Renderer imagePanel;

    void Start()
    {
        StartCoroutine(DownloadIDsAndImages());
    }

    IEnumerator DownloadIDsAndImages()
    {
        // Get IDs
        UnityWebRequest request = UnityWebRequest.Get(imageListUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load IDs: " + request.error);
            yield break;
        }

        string jsonResult = request.downloadHandler.text;

        // Parse array
        List<string> ids = ParseJsonArray(jsonResult);

        if (ids == null || ids.Count == 0)
        {
            Debug.LogError("No IDs found in the server response.");
            yield break;
        }

        // Pull imgs from ids
        foreach (string id in ids)
        {
            StartCoroutine(DownloadImageAndCreatePrefab(id));
        }
    }

    IEnumerator DownloadImageAndCreatePrefab(string id)
    {
        string imageUrl = imageFetchUrlBase + id;

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load image for ID {id}: {request.error}");
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);

        // Instantiate prefab
        GameObject newInstance = Instantiate(prefabToCopy, this.transform);
        ShowImage showImage = newInstance.GetComponentInChildren<ShowImage>();

        showImage.target = imagePanel;

        // Find Content/Background inside
        Transform backgroundTransform = newInstance.transform.Find("Content/Background");

        if (backgroundTransform != null)
        {
            Image targetImage = backgroundTransform.GetComponent<Image>();
            RectTransform backgroundRectTransform = backgroundTransform.GetComponent<RectTransform>();

            if (targetImage != null && backgroundRectTransform != null)
            {
                // Create sprite
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


    // Helper: Parse a raw JSON array like ["id1","id2"]
    List<string> ParseJsonArray(string json)
    {
        List<string> ids = new List<string>();

        json = json.Trim();
        if (json.StartsWith("[") && json.EndsWith("]"))
        {
            json = json.Substring(1, json.Length - 2);

            string[] rawObjects = json.Split(new string[] { "},{" }, StringSplitOptions.None);

            foreach (string raw in rawObjects)
            {
                string cleaned = raw.Replace("{", "").Replace("}", "").Replace("\"", "");
                string[] keyValue = cleaned.Split(':');

                if (keyValue.Length == 2 && keyValue[0].Trim() == "id")
                {
                    ids.Add(keyValue[1].Trim());
                }
            }
        }

        return ids;
    }


}
