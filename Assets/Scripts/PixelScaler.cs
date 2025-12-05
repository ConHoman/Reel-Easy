using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PerfectPixelScaler : MonoBehaviour
{
    public int referenceWidth = 320;
    public int referenceHeight = 180;

    void Start()
    {
        ApplyPerfectScaling();
    }

    void ApplyPerfectScaling()
    {
        int screenW = Screen.width;
        int screenH = Screen.height;

        // Find the largest whole-number scaling factor
        int scaleX = screenW / referenceWidth;
        int scaleY = screenH / referenceHeight;

        int finalScale = Mathf.Max(1, Mathf.Min(scaleX, scaleY));

        // Calculate the actually-used pixel resolution
        int renderW = referenceWidth * finalScale;
        int renderH = referenceHeight * finalScale;

        // Apply it to the screen resolution (windowed mode)
        Screen.SetResolution(renderW, renderH, false);
        Debug.Log($"Pixel-Perfect Scale: {finalScale}x ? {renderW}x{renderH}");
    }
}
