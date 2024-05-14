using UnityEngine;

public class DamageRangeBoost : PowerUpBase
{
    private float originalExplosionRadius;

    protected override void ApplyEffect(QuakeCharController player)
    {
        originalExplosionRadius = player.explosionManager.explosionRadius;
        player.explosionManager.explosionRadius *= 1.5f;
    }

    protected override void RemoveEffect(QuakeCharController player)
    {
        player.explosionManager.explosionRadius = originalExplosionRadius;
    }
}
