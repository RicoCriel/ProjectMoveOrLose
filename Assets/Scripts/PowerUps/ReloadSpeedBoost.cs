using System.Collections.Generic;
using UnityEngine;

public class ReloadSpeedBoost : PowerUpBase
{
    private Dictionary<PlayerMovement, float> originalReloadSpeeds = new Dictionary<PlayerMovement, float>();
    private HashSet<PlayerMovement> appliedPlayers = new HashSet<PlayerMovement>();
    private WeaponController weaponController;

    protected override void ApplyEffect(PlayerMovement player)
    {
        if (appliedPlayers.Contains(player))
        {
            Debug.Log("Effect already applied to this player. Skipping.");
            return;
        }

        if (!originalReloadSpeeds.ContainsKey(player))
        {
            weaponController = player.GetComponentInChildren<WeaponController>();
            if (weaponController != null)
            {
                float initialReloadSpeed = weaponController.canon.ReloadSpeed;
                Debug.Log($"Initial ReloadSpeed: {initialReloadSpeed}");

                originalReloadSpeeds[player] = initialReloadSpeed;

                weaponController.canon.ReloadSpeed = 0.5f;
                Debug.Log($"New ReloadSpeed after division: {weaponController.canon.ReloadSpeed}");

                weaponController.canon.AnimationSpeed *= 4f;
                // weaponController.cannonCooldownDuration /= 2f;
                appliedPlayers.Add(player);

                Debug.Log($"Reloadboost active. New reload speed: {weaponController.canon.ReloadSpeed}");
            }
        }
    }

    protected override void RemoveEffect(PlayerMovement player)
    {
        if (originalReloadSpeeds.TryGetValue(player, out float originalReloadSpeed))
        {
            Debug.Log($"Restoring original ReloadSpeed: {originalReloadSpeed}");
            weaponController = player.GetComponentInChildren<WeaponController>();
            if (weaponController != null)
            {
                weaponController.canon.ReloadSpeed = 1f;
                weaponController.canon.AnimationSpeed /= 4f;
                // weaponController.cannonCooldownDuration *= 2f;
                originalReloadSpeeds.Remove(player);
                appliedPlayers.Remove(player);
                Debug.Log($"Reloadboost deactivated. Restored reload speed: {weaponController.canon.ReloadSpeed}");
            }
        }
        else
        {
            Debug.Log("No original reload speed found for this player. Cannot remove effect.");
        }
    }
}
