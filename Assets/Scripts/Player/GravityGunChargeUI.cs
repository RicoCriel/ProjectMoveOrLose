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
    private Color minColor = Color.red;
    private Color maxColor = Color.green;
    private Tween fillTween;
    private float previousChargeTime; 

    private void Update()
    {
        if (ChargeTime != previousChargeTime)
        {
            UpdateFillAmount(ChargeTime);
            UpdateColor(ChargeTime);
            previousChargeTime = ChargeTime; 
        }
    }

    public void UpdateFillAmount(float chargeTime)
    {
        float duration = Mathf.Lerp(0.01f, 0.05f, 1 - chargeTime);
        fillTween = DOTween.To(() => chargeImage.fillAmount, x => chargeImage.fillAmount = x, chargeTime, duration);
        fillTween.OnKill(() => HandleTweenInterrupted());
    }

    private void UpdateColor(float chargeTime)
    {
        Color color = Color.Lerp(minColor, maxColor, chargeTime);
        float duration = Mathf.Lerp(0.01f, 0.05f, 1 - chargeTime);
        chargeImage.DOColor(color, duration);
    }

    public void ResetFillAmount()
    {
        chargeImage.DOKill();
        chargeImage.fillAmount = 0;
    }

    private void HandleTweenInterrupted()
    {
        if (fillTween != null && fillTween.IsActive())
        {
            fillTween.Kill();
        }
    }
}
