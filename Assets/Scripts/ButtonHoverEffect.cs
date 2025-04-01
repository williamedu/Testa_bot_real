using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonHoverEffect : MonoBehaviour
{
    private Vector3 originalScale;
    private Button button;
    public float animTime  = .2f;
    public float ScaleMultiplier = 1.2f;

    void Start()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    public void OnHoverEnter()
    {
        transform.DOScale(originalScale * ScaleMultiplier, animTime);
    }

    public void OnHoverExit()
    {
        transform.DOScale(originalScale, animTime);
    }

    private void OnClick()
    {
        transform.DOKill(); // Detiene cualquier animación en curso
        transform.DOScale(originalScale * 1.1f, 0.1f).SetEase(Ease.OutQuad)
            .OnComplete(() => transform.DOScale(originalScale, 0.1f).SetEase(Ease.OutQuad));
    }

    public void deactivatePopUp()
    {
      GetComponentInParent<popUpGO>().transform.DOScale(Vector3.zero, 0.2f);

    }
}
