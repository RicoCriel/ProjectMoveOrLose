using UnityEngine;

public class Invincibility : PowerUpBase
{
    protected override void ApplyEffect(QuakeCharController player)
    {
        player.isInvincible = true;
    }

    protected override void RemoveEffect(QuakeCharController player)
    {
        player.isInvincible = false;
    }
}
