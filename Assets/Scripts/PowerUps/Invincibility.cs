using UnityEngine;

public class Invincibility : PowerUpBase
{
    protected override void ApplyEffect(PlayerMovement player)
    {
        player.IsInvincible = true;
        Debug.Log("Invincibility active");
    }

    protected override void RemoveEffect(PlayerMovement player)
    {
        player.IsInvincible = false;
        Debug.Log("Invincibility deactivated");
    }
}
