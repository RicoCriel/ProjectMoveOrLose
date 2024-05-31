using DefaultNamespace.PowerUps.spawner;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DamageRangeBoost : PowerUpBase
{
    private Dictionary<PlayerMovement, float> originalExplosionRadiusVal = new Dictionary<PlayerMovement, float>();
    private HashSet<PlayerMovement> appliedPlayers = new HashSet<PlayerMovement>();

    protected override void ApplyEffect(PlayerMovement player)
    {
        if (appliedPlayers.Contains(player))
        {
            Debug.Log("Damage Range effect already applied to this player. Skipping.");
            return;
        }

        if (!originalExplosionRadiusVal.ContainsKey(player))
        {
            originalExplosionRadiusVal[player] = player.GetComponentInChildren<WeaponController>().explosionManager.explosionRadius;
            player.GetComponentInChildren<WeaponController>().explosionManager.explosionRadius *= 1.5f;
            appliedPlayers.Add(player);
            Debug.Log($"Damage Range active. New explosion radius: {player.GetComponentInChildren<WeaponController>().explosionManager.explosionRadius}");
        }
    }

    protected override void RemoveEffect(PlayerMovement player)
    {
        if (originalExplosionRadiusVal.TryGetValue(player, out float originalExplosionRadius))
        {
            player.GetComponentInChildren<WeaponController>().explosionManager.explosionRadius = originalExplosionRadius;
            originalExplosionRadiusVal.Remove(player);
            appliedPlayers.Remove(player);
            Debug.Log($"Damage Range deactivated. Restored explosion radius: {player.GetComponentInChildren<WeaponController>().explosionManager.explosionRadius}");
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
