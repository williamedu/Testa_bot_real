using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfitLossImageTracker : MonoBehaviour
{
    [Header("Text Reference")]
    [SerializeField] private TextMeshProUGUI profitLossText;

    [Header("Image Settings")]
    [SerializeField] private Image imageToChange;
    [SerializeField] private Sprite positiveImage;
    [SerializeField] private Sprite negativeImage;

    [Header("Update Settings")]
    [SerializeField] private float checkInterval = 0.5f;
    [SerializeField] private bool useTransition = true;
    [SerializeField] private float transitionDuration = 0.3f;

    private float currentProfitLoss = 0f;
    private float lastProfitLoss = 0f;
    private string profitPrefix = "$";
    private Coroutine transitionCoroutine;

    private void Start()
    {
        // Validate references
        if (profitLossText == null)
        {
            Debug.LogError("Profit/Loss TextMeshPro reference not set!");
            enabled = false;
            return;
        }

        if (imageToChange == null)
        {
            Debug.LogError("Image reference not set!");
            enabled = false;
            return;
        }

        if (positiveImage == null || negativeImage == null)
        {
            Debug.LogWarning("One or more image sprites are not set!");
        }

        // Start checking for profit/loss changes
        StartCoroutine(CheckProfitLossValue());
    }

    private IEnumerator CheckProfitLossValue()
    {
        while (true)
        {
            // Read the profit/loss text value
            if (profitLossText != null && !string.IsNullOrEmpty(profitLossText.text))
            {
                string text = profitLossText.text;

                // Remove prefix ($, €, etc.)
                if (text.StartsWith(profitPrefix))
                {
                    text = text.Substring(profitPrefix.Length);
                }

                // Try to parse the value
                if (float.TryParse(text, out float value))
                {
                    currentProfitLoss = value;

                    // Check if the value has changed
                    if (currentProfitLoss != lastProfitLoss)
                    {
                        UpdateImage();
                        lastProfitLoss = currentProfitLoss;
                    }
                }
            }

            // Wait for the next interval
            yield return new WaitForSeconds(checkInterval);
        }
    }

    private void UpdateImage()
    {
        // Determine which sprite to show based on profit/loss value
        Sprite targetSprite;

        if (currentProfitLoss >= 0)
        {
            targetSprite = positiveImage;
            Debug.Log("Profit is positive or zero: " + currentProfitLoss + ". Changing to positive image.");
        }
        else
        {
            targetSprite = negativeImage;
            Debug.Log("Profit is negative: " + currentProfitLoss + ". Changing to negative image.");
        }

        // Apply the new sprite with or without transition
        if (imageToChange != null && targetSprite != null)
        {
            if (useTransition)
            {
                // Stop previous transition if running
                if (transitionCoroutine != null)
                {
                    StopCoroutine(transitionCoroutine);
                }

                // Start new transition
                transitionCoroutine = StartCoroutine(TransitionImage(targetSprite));
            }
            else
            {
                // Apply immediately
                imageToChange.sprite = targetSprite;
            }
        }
    }

    private IEnumerator TransitionImage(Sprite targetSprite)
    {
        // Store the original color
        Color originalColor = imageToChange.color;

        // Fade out
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / (transitionDuration / 2));
            imageToChange.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Change the sprite
        imageToChange.sprite = targetSprite;

        // Fade in
        elapsedTime = 0f;
        while (elapsedTime < transitionDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / (transitionDuration / 2));
            imageToChange.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Ensure we end with the original alpha
        imageToChange.color = originalColor;
    }

    // Public method to manually update prefix if needed
    public void SetProfitPrefix(string newPrefix)
    {
        profitPrefix = newPrefix;
    }
}