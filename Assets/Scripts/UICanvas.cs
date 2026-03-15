using UnityEngine;
using UnityEngine.UI;

// Dedicated Screen Space Overlay canvas for all UI text.
// Renders after the Pixel Perfect Camera pass, so TMP text stays sharp.
public class UICanvas : MonoBehaviour
{
    public static UICanvas Instance;
    private Canvas _canvas;

    public static Canvas Get()
    {
        if (Instance == null)
            new GameObject("UICanvas").AddComponent<UICanvas>();
        return Instance._canvas;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(320, 180);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();
    }
}
