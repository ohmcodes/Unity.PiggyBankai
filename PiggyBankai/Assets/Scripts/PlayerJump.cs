using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] private PlayerMovementState playerMovementState;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float doubleJumpForce = 6f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(4f, 8f);
    [SerializeField] private float wallJumpMovementCooldown = 0.2f;
    [SerializeField] private int jumpCoinCost = 1;
    [SerializeField] private int doubleJumpCoinCost = 2;
    [SerializeField] private int wallJumpCoinCost = 3;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem doubleJumpParticles;
    [SerializeField] private GameManager gameManager;
    private PlayerMovement playerMovement;
    private float playerHalfHeight;
    private float playerHalfWidth;
    private bool canDoubleJump;

    private AudioSource jumpAudio;

    [Header("Jump Audio Settings")]
    [SerializeField] private float jumpVolumeMin = 0.7f;
    [SerializeField] private float jumpVolumeMax = 1.0f;
    [SerializeField] private float jumpPitchMin = 0.9f;
    [SerializeField] private float jumpPitchMax = 1.1f;

    private void Start()
    {
        playerHalfHeight = spriteRenderer.bounds.extents.y;
        playerHalfWidth = spriteRenderer.bounds.extents.x;
        playerMovement = GetComponent<PlayerMovement>();
        jumpAudio = GameObject.Find("JumpAudio").GetComponent<AudioSource>();
    }


    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Jump"))
        {
            if(gameManager.isDead) {return;}

            CheckJumpType();
        }
    }

    private void CheckJumpType()
    {
        bool isGrounded = GetIsGrounded();
        if(isGrounded)
        {
            playerMovementState.SetMoveState(PlayerMovementState.MovementState.Jump);
            Jump(jumpForce);
        }
        else
        {
            int direction = GetWallJumpDirection();
            if(direction == 0 && canDoubleJump && rb.velocity.y <= 0.1f)
            {
                DoubleJump();
            }
            else if(direction != 0)
            {
                WallJump(direction);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GetIsGrounded();
    }

    private int GetWallJumpDirection()
    {
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, playerHalfWidth + 0.1f, LayerMask.GetMask("Ground"));
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, playerHalfWidth + 0.1f, LayerMask.GetMask("Ground"));

        if(hitLeft)
        {
            return 1;
        }
        else if(hitRight)
        {
            return -1;
        }
        return 0;
    }

    private bool GetIsGrounded()
    {
        bool hit = Physics2D.Raycast(transform.position, Vector2.down, playerHalfHeight + 0.1f, LayerMask.GetMask("Ground"));
        if(hit)
        {
            canDoubleJump = true;
        }

        return hit;
    }

    private void Jump(float force)
    {
        rb.AddForce(new Vector2(0, force), ForceMode2D.Impulse);
        gameManager.coinCount = Mathf.Max(0, gameManager.coinCount - jumpCoinCost);
        gameManager.consumedCoins += jumpCoinCost;
        PlayJumpAudio();
    }
    private void PlayJumpAudio()
    {
        if (jumpAudio != null && jumpAudio.clip != null)
        {
            // Randomize volume and pitch for variety
            jumpAudio.volume = UnityEngine.Random.Range(jumpVolumeMin, jumpVolumeMax);
            jumpAudio.pitch = UnityEngine.Random.Range(jumpPitchMin, jumpPitchMax);

            // Play the jump sound
            jumpAudio.PlayOneShot(jumpAudio.clip);
        }
    }    
    private void DoubleJump()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        Jump(doubleJumpForce);
        canDoubleJump = false;
        doubleJumpParticles.Play();
        playerMovementState.SetMoveState(PlayerMovementState.MovementState.DoubleJump);
        gameManager.coinCount = Mathf.Max(0, gameManager.coinCount - doubleJumpCoinCost);
        gameManager.consumedCoins += doubleJumpCoinCost;
    }
    private void WallJump(int direction)
    {
        Vector2 force = wallJumpForce;
        force.x *= direction;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        playerMovement.wallJumpCooldown = wallJumpMovementCooldown;
        rb.AddForce(force, ForceMode2D.Impulse);
        playerMovementState.SetMoveState(PlayerMovementState.MovementState.WallJump);
        gameManager.coinCount = Mathf.Max(0, gameManager.coinCount - wallJumpCoinCost);
        gameManager.consumedCoins += wallJumpCoinCost;

         PlayJumpAudio();
    }
}
