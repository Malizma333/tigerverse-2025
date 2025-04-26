using UnityEngine;
using UnityEngine.UI; // Needed for Image

public class ShowImage : MonoBehaviour
{
    public GameObject imgSource; // This is your UI Image source
    public Renderer target; // This is your Quad's Renderer (e.g., MeshRenderer)

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
            // If the sprite is a part of an atlas, crop it
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
            // If the sprite covers the full texture
            return sprite.texture;
        }
    }
}
