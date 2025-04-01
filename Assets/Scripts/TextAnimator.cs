using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private AnimationType animationType = AnimationType.PopScale;
    [SerializeField] private Ease easeType = Ease.OutBack;

    [Header("Pop Animation Settings")]
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float popDuration = 0.3f;

    private TextMeshProUGUI textComponent;
    private Color originalColor;
    private Vector3 originalScale;
    private Sequence currentSequence;

    public enum AnimationType
    {
        FadeIn,
        PopScale,
        SlideIn,
        ColorPulse,
    }

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            originalColor = textComponent.color;
            originalScale = textComponent.transform.localScale;
        }
    }

    public void Animate(string newText = null, Color? newColor = null)
    {
        // Si no hay componente de texto, salir
        if (textComponent == null) return;

        // Actualizar el texto si se proporciona uno nuevo
        if (!string.IsNullOrEmpty(newText))
        {
            textComponent.text = newText;
        }

        // Actualizar el color si se proporciona uno nuevo
        if (newColor.HasValue)
        {
            textComponent.color = newColor.Value;
            originalColor = newColor.Value;
        }

        // Detener cualquier animación en curso
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }

        // Iniciar la animación apropiada
        switch (animationType)
        {
            case AnimationType.FadeIn:
                AnimateFadeIn();
                break;
            case AnimationType.PopScale:
                AnimatePopScale();
                break;
            case AnimationType.SlideIn:
                AnimateSlideIn();
                break;
            case AnimationType.ColorPulse:
                AnimateColorPulse();
                break;
        }
    }

    private void AnimateFadeIn()
    {
        // Reiniciar la opacidad
        Color startColor = textComponent.color;
        startColor.a = 0;
        textComponent.color = startColor;

        // Crear secuencia
        currentSequence = DOTween.Sequence();
        currentSequence.Append(textComponent.DOFade(originalColor.a, animationDuration).SetEase(easeType));
    }

    private void AnimatePopScale()
    {
        // Guardar escala original y resetear
        Vector3 startScale = originalScale;
        textComponent.transform.localScale = originalScale;

        // Crear secuencia para efecto "pop"
        currentSequence = DOTween.Sequence();

        // Escalar hacia arriba rápidamente
        currentSequence.Append(
            textComponent.transform.DOScale(originalScale * popScale, popDuration * 0.4f)
            .SetEase(Ease.OutQuad)
        );

        // Volver al tamaño original con un rebote
        currentSequence.Append(
            textComponent.transform.DOScale(originalScale, popDuration * 0.6f)
            .SetEase(Ease.OutBack)
        );
    }

    private void AnimateSlideIn()
    {
        // Preparar posición inicial
        Vector3 startPos = textComponent.transform.localPosition + new Vector3(-30, 0, 0);
        Vector3 endPos = textComponent.transform.localPosition;
        textComponent.transform.localPosition = startPos;

        // Animar el deslizamiento
        currentSequence = DOTween.Sequence();
        currentSequence.Append(
            textComponent.transform.DOLocalMove(endPos, animationDuration)
            .SetEase(easeType)
        );
    }

    private void AnimateColorPulse()
    {
        // Preparar colores
        Color targetColor = new Color(
            Mathf.Clamp01(originalColor.r + 0.3f),
            Mathf.Clamp01(originalColor.g + 0.3f),
            Mathf.Clamp01(originalColor.b + 0.3f),
            originalColor.a
        );

        // Crear secuencia de pulso
        currentSequence = DOTween.Sequence();
        currentSequence.Append(textComponent.DOColor(targetColor, animationDuration * 0.5f).SetEase(Ease.InOutQuad));
        currentSequence.Append(textComponent.DOColor(originalColor, animationDuration * 0.5f).SetEase(Ease.InOutQuad));
    }
}
