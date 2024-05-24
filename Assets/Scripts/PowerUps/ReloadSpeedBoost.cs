using System.Collections.Generic;
using UnityEngine;

public class ReloadSpeedBoost : PowerUpBase
{
    private Dictionary<PlayerMovement, float> originalReloadSpeeds = new Dictionary<PlayerMovement, float>();
    private HashSet<PlayerMovement> appliedPlayers = new HashSet<PlayerMovement>();

    protected override void ApplyEffect(PlayerMovement player)
    {
        if (appliedPlayers.Contains(player))
        {
            Debug.Log("Effect already applied to this player. Skipping.");
            return;
        }

        if (!originalReloadSpeeds.ContainsKey(player))
        {
            //originalReloadSpeeds[player] = player.shotGun.reloadSpeed;
            //player.shotGun.reloadSpeed /= 2;
            //appliedPlayers.Add(player);
            //Debug.Log($"Reloadboost active. New reload speed: {player.shotGun.reloadSpeed}");
        }
    }

    protected override void RemoveEffect(PlayerMovement player)
    {
        if (originalReloadSpeeds.TryGetValue(player, out float originalReloadSpeed))
        {
            //player.shotGun.reloadSpeed = originalReloadSpeed;
            //originalReloadSpeeds.Remove(player);
            //appliedPlayers.Remove(player);
            //Debug.Log($"Reloadboost deactivated. Restored reload speed: {player.shotGun.reloadSpeed}");
        }
        else
        {
            Debug.Log("No original reload speed found for this player. Cannot remove effect.");
        }
    }
}
