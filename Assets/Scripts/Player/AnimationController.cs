using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public enum AnimationState
{
    Idle,
    Running,
    Jumping,
}

public class AnimationController : MonoBehaviour, IPunObservable
{
    [Header("Player Animation Properties")]
    [SerializeField] private Animator robotAnimator;
    [SerializeField] private PlayerMovement playerMovement;

    private AnimationState robotState = AnimationState.Idle;
    private string previousState = "";
    private bool previousStateFlag;

    private Dictionary<AnimationState, string> stateToAnimation = new Dictionary<AnimationState, string>()
    {
        { AnimationState.Idle, "IsIdling" },
        { AnimationState.Running, "IsRunning" },
        { AnimationState.Jumping, "IsJumping" },
    };
    private Dictionary<PlayerMovement.PlayerState, AnimationState> playerToAnimationState = new Dictionary<PlayerMovement.PlayerState, AnimationState>()
    {
        { PlayerMovement.PlayerState.Idle, AnimationState.Idle },
        { PlayerMovement.PlayerState.Running, AnimationState.Running },
        { PlayerMovement.PlayerState.Jumping, AnimationState.Jumping },
    };

    private PhotonView view;

    void Start()
    {
        view = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (view.IsMine)
        {
            if (playerToAnimationState.TryGetValue(playerMovement.playerState, out AnimationState animState))
            {
                SetAnimationState(animState);
            }
        }
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

    public void SetAnimationState(AnimationState state)
    {
        robotState = state;
        UpdateAnimation();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(robotState);
        }
        else
        {
            // Network player, receive data
            AnimationState receivedState = (AnimationState)stream.ReceiveNext();
            if (receivedState != robotState)
            {
                SetAnimationState(receivedState);
            }
        }
    }
}