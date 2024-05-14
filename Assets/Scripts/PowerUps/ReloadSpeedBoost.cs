using UnityEngine;

public class ReloadSpeedBoost : PowerUpBase
{
    private float originalReloadSpeed;

    protected override void ApplyEffect(QuakeCharController player)
    {
        originalReloadSpeed = player.shotGun.reloadSpeed;
        player.shotGun.reloadSpeed *= 2;
    }

    protected override void RemoveEffect(QuakeCharController player)
    {
        player.shotGun.reloadSpeed = originalReloadSpeed;
    }
}
