using UnityEngine;

public class OpacityChanger : MonoBehaviour
{
    public Renderer cubeRenderer;

    private void Awake()
    {
        Material cubeMaterial = cubeRenderer.material;

        // Make sure the shader supports transparency
        cubeMaterial.SetFloat("_Mode", 3); // 3 = Transparent for Standard shader
        cubeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        cubeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        cubeMaterial.SetInt("_ZWrite", 0);
        cubeMaterial.DisableKeyword("_ALPHATEST_ON");
        cubeMaterial.EnableKeyword("_ALPHABLEND_ON");
        cubeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        cubeMaterial.renderQueue = 3000;
    }

    public void SetOpacity(float value)
    {
        Material cubeMaterial = cubeRenderer.material;

        if (cubeMaterial != null)
        {
            Color color = cubeMaterial.color;
            color.a = Mathf.Clamp01(value); // Ensure between 0 and 1
            cubeMaterial.color = color;
        }
    }
}
