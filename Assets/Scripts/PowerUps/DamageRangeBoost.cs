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
            originalExplosionRadiusVal[player] = player.GetComponentInChildren<Canon>().RadiusMultiplier;
            player.GetComponentInChildren<Canon>().RadiusMultiplier *= 3f;
            appliedPlayers.Add(player);
            Debug.Log($"Damage RadiusBoost active. New explosion radius: {player.GetComponentInChildren<Canon>().RadiusMultiplier}");
        }
    }

    protected override void RemoveEffect(PlayerMovement player)
    {
        if (originalExplosionRadiusVal.TryGetValue(player, out float originalExplosionRadius))
        {
            player.GetComponentInChildren<Canon>().RadiusMultiplier = originalExplosionRadius;
            originalExplosionRadiusVal.Remove(player);
            appliedPlayers.Remove(player);
            Debug.Log($"Damage RadiusBoost deactivated. Restored explosion radius: {player.GetComponentInChildren<Canon>().RadiusMultiplier}");
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
