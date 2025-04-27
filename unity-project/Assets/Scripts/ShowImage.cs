using UnityEngine;
using UnityEngine.UI;

public class ShowImage : MonoBehaviour
{
    public GameObject imgSource;
    public Renderer target;
    private GameObject scaleManager;
    private float original = 0.25f;

    public void OnValueChanged(bool isOn)
    {
        if (imgSource != null && target != null)
        {
            Image sourceImage = imgSource.GetComponent<Image>();
            if (sourceImage != null && sourceImage.sprite != null)
            {
                Texture2D texture = SpriteToTexture2D(sourceImage.sprite);
                target.material.mainTexture = texture;

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
