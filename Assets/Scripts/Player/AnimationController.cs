using UnityEngine;
using System.Collections.Generic;

public enum AnimationState
{
    Idle,
    Running,
    Jumping,
}

public class AnimationController : MonoBehaviour
{
    [Header("Player Animation Properties")]
    [SerializeField] private Animator robotAnimator;
    [SerializeField] private SkinnedMeshRenderer robotMesh;

    private AnimationState robotState = AnimationState.Idle;
    private string previousState = "";
    private bool previousStateFlag;
    public bool hideMesh;

    private Dictionary<AnimationState, string> stateToAnimation = new Dictionary<AnimationState, string>()
    {
        { AnimationState.Idle, "IsIdling" },
        { AnimationState.Running, "IsRunning" },
        { AnimationState.Jumping, "IsJumping" },
    };

    private void Start()
    {
        if (hideMesh)
        {
            robotMesh.enabled = false;
        }
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        string nextState = stateToAnimation[robotState];
        bool nextStateFlag = !string.IsNullOrEmpty(nextState);

        if (previousStateFlag)
        {
            robotAnimator.SetBool(previousState, false);
        }

        if (nextStateFlag)
        {
            robotAnimator.SetBool(nextState, true);
        }

        previousStateFlag = nextStateFlag;
        previousState = nextState;
    }

    public void SetPlayerState(AnimationState state)
    {
        robotState = state;
    }
}

