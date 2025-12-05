using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class BubbleGameManager : MonoBehaviour
{
    // Reference to the player's FishingController (assign in Inspector)
    public FishingController fishingController;

    public GameObject bubblePrefab;
    public RectTransform panel;
    public TMP_Text countdownText;

    public int bubblesNeeded = 3;
    public int allowedMisses = 3;

    int popped = 0;
    int missed = 0;

    public void StartBubbleGame()
    {
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        panel.gameObject.SetActive(true);
        countdownText.enabled = true;

        countdownText.text = "3..";
        yield return new WaitForSeconds(1f);

        countdownText.text = "2..";
        yield return new WaitForSeconds(1f);

        countdownText.text = "1..";
        yield return new WaitForSeconds(1f);

        countdownText.text = "POP!";
        yield return new WaitForSeconds(0.5f);

        countdownText.enabled = false;

        popped = 0;
        missed = 0;

        StartCoroutine(BubbleLoop());
    }

    IEnumerator BubbleLoop()
    {
        // Force correct panel pivot/anchors so bubble positions aren't skewed
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);

        float panelW = panel.rect.width;
        float panelH = panel.rect.height;

        while (popped < bubblesNeeded && missed < allowedMisses)
        {
            GameObject bubble = Instantiate(bubblePrefab, panel);
            RectTransform rt = bubble.GetComponent<RectTransform>();

            float halfW = rt.rect.width * 0.5f;
            float halfH = rt.rect.height * 0.5f;

            // NEW — spawn centered, not just top-right
            float x = Random.Range(-panelW / 2 + halfW, panelW / 2 - halfW);
            float y = Random.Range(-panelH / 2 + halfH, panelH / 2 - halfH);

            rt.anchoredPosition = new Vector2(x, y);

            bool poppedThisBubble = false;
            float life = 1.2f;

            // CLICK POP — remove bubble on click
            Button clicker = bubble.AddComponent<Button>();
            clicker.transition = Selectable.Transition.None;
            clicker.onClick.AddListener(() =>
            {
                poppedThisBubble = true;
                popped++;
                Destroy(bubble);
            });

            // Bubble lifetime
            while (life > 0f && !poppedThisBubble)
            {
                life -= Time.deltaTime;
                yield return null;
            }

            if (!poppedThisBubble)
            {
                missed++;
                Destroy(bubble);
            }

            yield return new WaitForSeconds(0.15f);
        }

        EndGame();
    }

    void EndGame()
    {
        panel.gameObject.SetActive(false);

        if (popped >= bubblesNeeded)
            fishingController.CatchFishSuccess();
        else
            fishingController.CatchFishFail();
    }
}
