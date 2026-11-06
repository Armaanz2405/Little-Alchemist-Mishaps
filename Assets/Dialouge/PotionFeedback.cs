
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PotionFeedback : MonoBehaviour
{
    [Header("Feedback UI")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Testing")]
    [SerializeField] private bool enableTestKey = true;

    private void Start()
    {
        HideFeedback();
    }

    private void Update()
    {
        // Press ] to test a random potion reaction
        if (enableTestKey &&
            Keyboard.current != null &&
            Keyboard.current.rightBracketKey.wasPressedThisFrame)
        {
            int randomScore = Random.Range(0, 101);

            Debug.Log("[PotionFeedback] Test key pressed! Random score: " + randomScore);

            ShowFeedback(randomScore);
        }
    }

    public void ShowFeedback(int score)
    {
        if (feedbackPanel == null || feedbackText == null)
        {
            Debug.LogWarning("[PotionFeedback] UI references are missing!");
            return;
        }

        if (score >= 90)
        {
            feedbackText.text = "Well done! Magnificent!";
        }
        else if (score >= 60)
        {
            feedbackText.text = "Well done, young apothecary.";
        }
        else
        {
            feedbackText.text = "Hmm... This is not quite what I asked for.";
        }

        feedbackPanel.SetActive(true);

        Debug.Log("[PotionFeedback] Reaction displayed! Score: " + score);
    }

    public void HideFeedback()
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
    }
}
