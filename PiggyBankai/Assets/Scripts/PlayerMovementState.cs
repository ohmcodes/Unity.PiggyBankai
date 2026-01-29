using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerMovementState : MonoBehaviour
{
    public enum MovementState
    {
        Idle,
        Run,
        Jump,
        Fall,
        DoubleJump,
        WallJump
    }
    public MovementState currentState {get; private set;}
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    private const string idleAnim = "Idle";
    private const string runAnim = "Run";
    private const string jumpAnim = "Jump";
    private const string fallAnim = "Fall";
    private const string doubleJumpAnim = "DoubleJump";
    private const string wallJumpAnim = "WallJump";
    public static Action<MovementState> OnPlayerMovementStateChanged;
    private float xPosLastFrame;

    private void Update()
    {
        if(transform.position.x == xPosLastFrame && rb.velocity.y == 0)
        {
            SetMoveState(MovementState.Idle);
        }
        else if(transform.position.x != xPosLastFrame && rb.velocity.y == 0)
        {
            SetMoveState(MovementState.Run);
        }
        else if(rb.velocity.y < 0)
        {
            SetMoveState(MovementState.Fall);
        }

        xPosLastFrame = transform.position.x;
    }

    public void SetMoveState(MovementState state)
    {
        if(state == currentState) {return;}   

        switch(state)
        {
            case MovementState.Idle:
                animator.Play(idleAnim);
                break;
            case MovementState.Run:
                animator.Play(runAnim);
                break;
            case MovementState.Jump:
                animator.Play(jumpAnim);
                break;
            case MovementState.Fall:
                animator.Play(fallAnim);
                break;
            case MovementState.DoubleJump:
                animator.Play(doubleJumpAnim);
                break;
            case MovementState.WallJump:
                animator.Play(wallJumpAnim);
                break;
            default:
                animator.Play(idleAnim);
                break;
        }

        OnPlayerMovementStateChanged?.Invoke(state);
        currentState = state;
    }
}
