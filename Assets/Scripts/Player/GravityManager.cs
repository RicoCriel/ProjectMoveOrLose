using UnityEngine;
using System.Collections.Generic;

public enum GravityManagerState
{
    Up,  //Gravity that pulls towards the top face.
    Down, //Gravity that pulls towards the bottom face.
    Forward, //Gravity: Gravity that pulls towards the front face.
    Backward, //Gravity: Gravity that pulls towards the back face.
    Left, //Gravity: Gravity that pulls towards the left face.
    Right, //Gravity: Gravity that pulls towards the right face.
}

public class GravityManager : MonoBehaviour
{
    public GravityState currentGravityState = GravityState.Down;
    private GravityState previousGravityState;
    public float gravity = 20.0f;

    private Dictionary<GravityState, Quaternion> gravityRotations;
    private Dictionary<GravityState, Vector3> gravityDirections;

    private void Start()
    {
        InitializeGravityDictionaries();
        previousGravityState = currentGravityState;
    }

    private void Update()
    {
        if (previousGravityState != currentGravityState)
        {
            UpdateGravityState();
        }
    }

    private void InitializeGravityDictionaries()
    {
        gravityRotations = new Dictionary<GravityState, Quaternion>
        {
            { GravityState.Up, Quaternion.Euler(0, 0, 180) },
            { GravityState.Down, Quaternion.identity },
            { GravityState.Forward, Quaternion.Euler(90, 0, -180) },
            { GravityState.Backward, Quaternion.Euler(90, 0, 0) },
            { GravityState.Left, Quaternion.Euler(0, 0, -90) },
            { GravityState.Right, Quaternion.Euler(0, 0, 90) }
        };

        gravityDirections = new Dictionary<GravityState, Vector3>
        {
            { GravityState.Up, Vector3.up },
            { GravityState.Down, Vector3.down },
            { GravityState.Forward, Vector3.forward },
            { GravityState.Backward, Vector3.back },
            { GravityState.Left, Vector3.left },
            { GravityState.Right, Vector3.right }
        };
    }

    private void UpdateGravityState()
    {
        SetGravityDirection();
        UpdateGravityRotation();
        previousGravityState = currentGravityState;
    }

    private void UpdateGravityRotation()
    {
        if (gravityRotations.TryGetValue(currentGravityState, out Quaternion rotation))
        {
            transform.localRotation = rotation;
        }
        else
        {
            Debug.LogWarning("Rotation not found for current gravity state.");
        }
    }

    private void SetGravityDirection()
    {
        if (gravityDirections.TryGetValue(currentGravityState, out Vector3 direction))
        {
            Physics.gravity = direction * gravity;
        }
        else
        {
            Debug.LogWarning("Gravity state not found in dictionary.");
        }
    }
}
