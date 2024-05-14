using UnityEngine;

public class SpeedBoost : PowerUpBase
{
    private float originalSpeed;

    protected override void ApplyEffect(QuakeCharController player)
    {
        originalSpeed = player.moveSpeed;
        player.moveSpeed *= 2;
    }

    protected override void RemoveEffect(QuakeCharController player)
    {
        player.moveSpeed = originalSpeed;
    }
}
