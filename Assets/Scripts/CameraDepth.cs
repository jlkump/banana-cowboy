using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraDepth : MonoBehaviour
{
    [SerializeField]
    DepthTextureMode depthTextureMode;

    public Shader edgeShader;
    private Material edgeMat;

    private void OnValidate()
    {
        SetCameraDepthTextureMode();
    }

    private void Awake()
    {
        SetCameraDepthTextureMode();
    }

    // code adapted from Acerola's gooch shader
    private void Start()
    {
        if (edgeMat == null) { 
            edgeMat = new Material(edgeShader);
            // temporary material 
            edgeMat.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, edgeMat);
    }

    private void SetCameraDepthTextureMode()
    {
        GetComponent<Camera>().depthTextureMode = depthTextureMode;
    }
}