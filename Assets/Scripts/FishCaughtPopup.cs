using UnityEngine;
using TMPro;
using System.Collections;

public class FishCaughtPopup : MonoBehaviour
{
    public Transform player;
    public TMP_Text textObj;
    public Vector3 offset = new Vector3(0, 1f, 0);

    public float fadeTime = 0.3f;
    public float stayTime = 1f;

    CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f; // start invisible
    }

    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(player.position + offset);
        }
    }

    public void ShowMessage(string message)
    {
        if (textObj == null)
        {
            Debug.LogError("Popup has no TMP_Text assigned!");
            return;
        }

        textObj.text = message;
        StopAllCoroutines();
        StartCoroutine(PopupRoutine());
    }

    IEnumerator PopupRoutine()
    {
        cg.alpha = 0f;

        // Fade in
        float t = 0;
        while (t < fadeTime)
        {
            cg.alpha = Mathf.Lerp(0, 1, t / fadeTime);
            t += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 1;

        yield return new WaitForSeconds(stayTime);

        // Fade out
        t = 0;
        while (t < fadeTime)
        {
            cg.alpha = Mathf.Lerp(1, 0, t / fadeTime);
            t += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 0;
    }
}
