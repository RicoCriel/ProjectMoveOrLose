using UnityEngine;

public class Invincibility : PowerUpBase
{
    protected override void ApplyEffect(PlayerMovement player)
    {
        //player.isInvincible = true;
        Debug.Log("Invincibility active");
    }

    protected override void RemoveEffect(PlayerMovement player)
    {
        //player.isInvincible = false;
        Debug.Log("Invincibility deactivated");
    }
}
