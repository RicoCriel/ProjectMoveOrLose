using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CanonUI : MonoBehaviour
{
    [SerializeField]
    private Image canonImage;

    private Sequence fadeSequence;

    public void StartFading()
    {
        if (fadeSequence == null || !fadeSequence.IsActive())
        {
            fadeSequence = DOTween.Sequence();
            fadeSequence.Append(canonImage.DOFade(0f, 0.5f));
            fadeSequence.Append(canonImage.DOFade(1f, 0.5f));
            fadeSequence.SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void StopFading()
    {
        if (fadeSequence != null && fadeSequence.IsActive())
        {
            fadeSequence.Kill();
            canonImage.DOFade(1f, 0.1f); 
        }
    }
}
