using UnityEngine;

public class Invincibility : PowerUpBase
{
    protected override void ApplyEffect(QuakeCharController player)
    {
        player.isInvincible = true;
        Debug.Log("Invincibility active");
    }

    protected override void RemoveEffect(QuakeCharController player)
    {
        player.isInvincible = false;
        Debug.Log("Invincibility deactivated");
    }
}
