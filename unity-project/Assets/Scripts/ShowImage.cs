using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class ShowImage : MonoBehaviour
{
    public GameObject imgSource;
    public GameObject imgDataHolder;
    public GameObject target;
    private float original = 0.25f;

    public void OnValueChanged(bool isOn)
    {
        if (imgSource != null && target != null)
        {
            Image sourceImage = imgSource.GetComponent<Image>();
            Debug.Log(sourceImage);
            if (sourceImage != null && sourceImage.sprite != null)
            {
                Texture2D texture = SpriteToTexture2D(sourceImage.sprite);
                ImageInfoHolder curr = imgDataHolder.GetComponent<ImageInfoHolder>();
                Transform scaleParent = target.transform.Find("ScaleParent");

                if (scaleParent != null && curr != null)
                {
                    Transform originalChild = scaleParent.Find("Original");
                    Transform detailedChild = scaleParent.Find("Detailed");
                    Transform texturedChild = scaleParent.Find("Textured");

                    Debug.Log(originalChild);
                    Debug.Log(detailedChild);

                    if (originalChild != null)
                    {
                        Renderer ogRenderer = originalChild.GetComponent<Renderer>();
                        if (ogRenderer != null)
                        {
                            StartCoroutine(DownloadAndSetTexture(curr.imageData.original, ogRenderer));
                        }
                    }

                    if (detailedChild != null)
                    {
                        Renderer detailedRenderer = detailedChild.GetComponent<Renderer>();
                        if (detailedRenderer != null)
                        {
                            StartCoroutine(DownloadAndSetTexture(curr.imageData.detailed, detailedRenderer));
                        }
                    }

                    if (texturedChild != null)
                    {
                        Renderer texturedRenderer = texturedChild.GetComponent<Renderer>();
                        if (texturedRenderer != null)
                        {
                            StartCoroutine(DownloadAndSetTexture(curr.imageData.textured, texturedRenderer));
                        }
                    }
                }

                float width = sourceImage.sprite.rect.width;
                float height = sourceImage.sprite.rect.height;
                float aspect = width / height;

                Vector3 newScale = new Vector3(0,0,0);
                newScale.z = 1f;

                if (aspect >= 1f)
                {
                    newScale.x = original;
                    newScale.y = original / aspect;
                }
                else
                {
                    newScale.y = original;
                    newScale.x = original * aspect;
                }

                target.transform.localScale = newScale * target.transform.localScale.z;
            }
            else
            {
                Debug.LogWarning("Source Image or Sprite is missing.");
            }
        }
    }

    private IEnumerator DownloadAndSetTexture(string id, Renderer rendererTarget)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("Missing ID for detailed or textured image.");
            yield break;
        }

        string url = $"https://tigerverse-2025.onrender.com/image/{id}";
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download image: " + request.error);
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        texture.mipMapBias = 0;
        texture.filterMode = FilterMode.Point;
        texture.anisoLevel = 0;
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);

        rendererTarget.material.mainTexture = texture;
    }


    private Texture2D SpriteToTexture2D(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width || sprite.rect.height != sprite.texture.height)
        {
            Texture2D croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] pixels = sprite.texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
            return croppedTexture;
        }
        else
        {
            return sprite.texture;
        }
    }
}
