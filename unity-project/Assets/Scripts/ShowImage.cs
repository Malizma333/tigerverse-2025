using UnityEngine;
using UnityEngine.UI;

public class ShowImage : MonoBehaviour
{
    public GameObject imgSource; // UI Image source
    public Renderer target;      // Quad's Renderer

    public void OnValueChanged(bool isOn)
    {
        Debug.Log("pressed");
        if (imgSource != null && target != null)
        {
            Debug.Log("in");
            Image sourceImage = imgSource.GetComponent<Image>();
            if (sourceImage != null && sourceImage.sprite != null)
            {
                // Create a Texture2D from the sprite
                Texture2D texture = SpriteToTexture2D(sourceImage.sprite);

                // Apply it to the Quad's material
                target.material.mainTexture = texture;

                // Resize the quad to match the sprite's aspect ratio
                float width = sourceImage.sprite.rect.width;
                float height = sourceImage.sprite.rect.height;
                float aspect = width / height;

                Vector3 originalScale = target.transform.localScale;
                float baseScale = Mathf.Min(originalScale.x, originalScale.y);

                Vector3 newScale = originalScale;

                if (aspect >= 1f)
                {
                    // Wider than tall
                    newScale.x = baseScale;
                    newScale.y = baseScale / aspect;
                }
                else
                {
                    // Taller than wide
                    newScale.y = baseScale;
                    newScale.x = baseScale * aspect;
                }

                target.transform.localScale = newScale;
            }
            else
            {
                Debug.LogWarning("Source Image or Sprite is missing.");
            }
        }
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
