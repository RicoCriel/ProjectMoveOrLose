using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GravityUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI gravityText;

    public void DisplayGravityUI()
    {
        gravityText.text = "Charging..";
    }

    public void HideGravityUI()
    {
        gravityText.text = string.Empty;
    }
}
