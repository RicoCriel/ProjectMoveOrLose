using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : PowerUpBase
{
    private Dictionary<QuakeCharController, float> originalSpeeds = new Dictionary<QuakeCharController, float>();
    private HashSet<QuakeCharController> appliedPlayers = new HashSet<QuakeCharController>();

    protected override void ApplyEffect(QuakeCharController player)
    {
        if (appliedPlayers.Contains(player))
        {
            Debug.Log("Speed boost already applied to this player. Skipping.");
            return;
        }

        if (!originalSpeeds.ContainsKey(player))
        {
            originalSpeeds[player] = player.moveSpeed;
            player.moveSpeed *= 1.5f; 
            appliedPlayers.Add(player);
            Debug.Log($"Speed boost active. New speed: {player.moveSpeed}");
        }
    }

    protected override void RemoveEffect(QuakeCharController player)
    {
        if (originalSpeeds.TryGetValue(player, out float originalSpeed))
        {
            player.moveSpeed = originalSpeed;
            originalSpeeds.Remove(player);
            appliedPlayers.Remove(player);
            Debug.Log($"Speed boost deactivated. Restored speed: {player.moveSpeed}");
        }
        else
        {
            Debug.Log("No original speed found for this player. Cannot remove effect.");
        }
    }
}
