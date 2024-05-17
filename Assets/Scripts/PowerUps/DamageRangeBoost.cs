using DefaultNamespace.PowerUps.spawner;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DamageRangeBoost : PowerUpBase
{
    private Dictionary<QuakeCharController, float> originalExplosionRadiusVal = new Dictionary<QuakeCharController, float>();
    private HashSet<QuakeCharController> appliedPlayers = new HashSet<QuakeCharController>();

    protected override void ApplyEffect(QuakeCharController player)
    {
        if (appliedPlayers.Contains(player))
        {
            Debug.Log("Damage Range effect already applied to this player. Skipping.");
            return;
        }

        if (!originalExplosionRadiusVal.ContainsKey(player))
        {
            originalExplosionRadiusVal[player] = player.explosionManager.explosionRadius;
            player.explosionManager.explosionRadius *= 1.5f;
            appliedPlayers.Add(player);
            Debug.Log($"Damage Range active. New explosion radius: {player.explosionManager.explosionRadius}");
        }
    }

    protected override void RemoveEffect(QuakeCharController player)
    {
        if (originalExplosionRadiusVal.TryGetValue(player, out float originalExplosionRadius))
        {
            player.explosionManager.explosionRadius = originalExplosionRadius;
            originalExplosionRadiusVal.Remove(player);
            appliedPlayers.Remove(player);
            Debug.Log($"Damage Range deactivated. Restored explosion radius: {player.explosionManager.explosionRadius}");
        }
        else
        {
            Debug.Log("No original explosion radius found for this player. Cannot remove effect.");
        }
    }

    public event EventHandler<SpawnerDoneEventArgs> SpawnerDone;
}

public class SpawnerDoneEventArgs : EventArgs
{
    public PowerUpSpawner Spawner{ get; }

    public SpawnerDoneEventArgs(PowerUpSpawner spawner)
    {
        Spawner = spawner;
    }
}
