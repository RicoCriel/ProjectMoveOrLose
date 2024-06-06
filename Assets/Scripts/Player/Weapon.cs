using UnityEngine;
using Photon.Pun;

public abstract class Weapon : MonoBehaviourPun
{
    public bool IsSecondaryGun { get; private set; } = false;

    public void ActivateAsSecondary()
    {
        IsSecondaryGun = true;
        OnActivate();
    }

    public void DeactivateAsSecondary()
    {
        IsSecondaryGun = false;
        OnDeactivate();
    }

    protected abstract void OnActivate();
    protected abstract void OnDeactivate();
}
