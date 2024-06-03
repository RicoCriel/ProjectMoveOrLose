using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GravityGunChargeUI : MonoBehaviour
{
    public float ChargeTime;

    [SerializeField]
    private Image chargeImage;
    [SerializeField]
    private float maxChargeTime;
    [SerializeField]
    private Color minColor = Color.red;
    [SerializeField]
    private Color maxColor = Color.green;
    [SerializeField]
    private float scaleChangeDuration;
    [SerializeField]
    private float maxScaleChangeAmount;
    private bool isPulsing;

    private void Update()
    {
        UpdateFillAmount(ChargeTime);
        UpdateColor(ChargeTime);
        UpdatePulse();
    }

    private void UpdatePulse()
    {
        if (chargeImage.fillAmount == 1 && !isPulsing)
        {
            StartPulse();
        }
        else if (chargeImage.fillAmount < 1 && isPulsing)
        {
            StopPulse();
        }
    }

    private void UpdateFillAmount(float chargeTime)
    {
        float normalizedChargeTime = Mathf.Clamp01(chargeTime / maxChargeTime);
        float duration = Mathf.Lerp(0.01f, 0.05f, 1 - normalizedChargeTime); 
        DOTween.To(() => chargeImage.fillAmount, x => chargeImage.fillAmount = x, chargeTime, duration);
    }

    private void UpdateColor(float chargeTime)
    {
        Color color = Color.Lerp(minColor, maxColor, chargeTime);
        float duration = Mathf.Lerp(0.01f, 0.05f, 1 - chargeTime);
        chargeImage.DOColor(color, duration);
    }

    private void StartPulse()
    {
        isPulsing = true;
        Pulse();
    }

    private void StopPulse()
    {
        isPulsing = false;
        chargeImage.transform.localScale = Vector3.one;
        DOTween.Kill(chargeImage.transform);
        chargeImage.color = minColor;
    }

    private void Pulse()
    {
        if (isPulsing)
        {
            chargeImage.transform.DOScale(maxScaleChangeAmount,
                scaleChangeDuration).SetLoops(-1, LoopType.Yoyo);
        }
    }
}
