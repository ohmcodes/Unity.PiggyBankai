using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	#region Variables
	[Header("References")]
    [SerializeField] private float speed = 5f;
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private Animator animator;
	[SerializeField] private float unitsPerCoin = 5f;
	[SerializeField] private PlayerMovementState playerMovementState;
	[SerializeField] private ParticleSystem coinParticles;
	[SerializeField] private Rigidbody2D rb;
	[SerializeField] private GameManager gameManager;

	public float wallJumpCooldown {get; set;}
	public Transform bulletSpawnPoint;
	public GameObject bulletPrefab;
	public float bulletSpeed = 10f;
	public DistanceManager distanceManager;

    private Vector2 movement;
	private Vector2 initialPosition;
	private float maxX;
	private float coinProgress = 0f;
	private Vector2 screenBounds;
	private float playerHalfHeight;
	private float playerHalfWidth;
	private float xPosLastFrame;
	private float distanceWalked = 0f;
	private Vector2 particlesStartPos;
	#endregion

	private AudioSource footstepsAudio;
	[SerializeField] private AudioSource shootAudio;

	[Header("Footsteps Settings")]
	[SerializeField] private float footstepsVolumeMin = 0.3f;
	[SerializeField] private float footstepsVolumeMax = 0.6f;
	[SerializeField] private float footstepsPitchMin = 0.9f;
	[SerializeField] private float footstepsPitchMax = 1.1f;
	[SerializeField] private float footstepsInterval = 0.3f; // Time between footstep sounds

	[Header("Shoot Audio Settings")]
	[SerializeField] private float shootPitchMin = 0.95f;
	[SerializeField] private float shootPitchMax = 1.05f;

	private float lastFootstepTime = 0f;
	private bool wasMovingLastFrame = false;
	private bool wasFirePressedLastFrame = false;

	private void PlayFootstepSound()
	{
		if (footstepsAudio != null && footstepsAudio.clip != null)
		{
			// Randomize volume and pitch for variety
			footstepsAudio.volume = UnityEngine.Random.Range(footstepsVolumeMin, footstepsVolumeMax);
			footstepsAudio.pitch = UnityEngine.Random.Range(footstepsPitchMin, footstepsPitchMax);

			// Play the footstep sound
			footstepsAudio.PlayOneShot(footstepsAudio.clip);
		}
	}
	private bool IsGrounded()
	{
		return Physics2D.Raycast(transform.position, Vector2.down, playerHalfHeight + 0.1f, LayerMask.GetMask("Ground"));
	}
    private void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
		playerHalfWidth = spriteRenderer.bounds.extents.x;
		playerHalfHeight = spriteRenderer.bounds.extents.y;
		xPosLastFrame = transform.position.x;
		initialPosition = transform.position;
		maxX = transform.position.x;
		particlesStartPos = coinParticles.transform.localPosition;
		footstepsAudio = GameObject.Find("FootstepsAudio").GetComponent<AudioSource>();

    }
    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        //ClampMovement();
		StartStopParticles();
		FlipCharacterX();
		

		float deltaX = transform.position.x - xPosLastFrame;
		coinProgress += Mathf.Abs(deltaX);
		if (transform.position.x > maxX)
		{
			maxX = transform.position.x;
		}
		distanceWalked = maxX - initialPosition.x;
		//distanceManager.distanceWalked = distanceWalked;
		gameManager.distanceWalked = distanceWalked;
		

		if (playerMovementState.currentState != PlayerMovementState.MovementState.Jump &&
		    playerMovementState.currentState != PlayerMovementState.MovementState.DoubleJump &&
		    playerMovementState.currentState != PlayerMovementState.MovementState.WallJump)
		{
			bool hitRight = Physics2D.Raycast(transform.position, Vector2.right, playerHalfWidth + 0.1f, LayerMask.GetMask("Ground"));
			bool hitLeft = Physics2D.Raycast(transform.position, Vector2.left, playerHalfWidth + 0.1f, LayerMask.GetMask("Ground"));
			if (!hitRight && !hitLeft)
			{
				while (coinProgress >= unitsPerCoin)
				{
					gameManager.coinCount--;
					gameManager.consumedCoins++;
					coinProgress -= unitsPerCoin;
				}
			}
		}

		if(wallJumpCooldown > 0)
		{
			wallJumpCooldown -= Time.deltaTime;
		}

		if(Input.GetButtonDown("Fire1") || (Input.GetAxis("Fire1") > 0.5f && !wasFirePressedLastFrame))
        {
    		HandleFiring();
        }

        wasFirePressedLastFrame = Input.GetAxis("Fire1") > 0.5f;

        xPosLastFrame = transform.position.x;
    
    }

    private void HandleFiring()
    {
        if (gameManager.isDead) { return; }

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        rb.velocity = direction * bulletSpeed;
        gameManager.coinCount--;
        gameManager.consumedCoins++;

        // Play shoot audio with random pitch
        if (shootAudio != null && shootAudio.clip != null)
        {
            shootAudio.pitch = UnityEngine.Random.Range(shootPitchMin, shootPitchMax);
            shootAudio.PlayOneShot(shootAudio.clip);
        }
    }

    private void StartStopParticles()
	{
		if(playerMovementState.currentState == PlayerMovementState.MovementState.Run)
		{
			if(!coinParticles.isPlaying)
			{
				coinParticles.Play();
			}
		}
		else
		{
			if(coinParticles.isPlaying)
			{
				coinParticles.Stop();
			}
		}
	}

	private void FlipCharacterX()
	{
		if(gameManager.isDead) {return;}

		float input = Input.GetAxisRaw("Horizontal");
		if(input > 0 && (transform.position.x > xPosLastFrame))
		{
			spriteRenderer.flipX = false;
			coinParticles.transform.localPosition = particlesStartPos;
			coinParticles.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		}
		else if(input < 0 && (transform.position.x < xPosLastFrame))
		{
			spriteRenderer.flipX = true;
			Vector2 particlesPos = particlesStartPos;
			particlesPos.x = -particlesPos.x;
			coinParticles.transform.localPosition = particlesPos;
			coinParticles.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
		}
		

		
	}

    private void HandleMovement()
    {
		if(gameManager.isDead) {return;}

		if(wallJumpCooldown > 0f) {return;}

        float input = Input.GetAxisRaw("Horizontal");
        movement.x = input * speed * Time.deltaTime;
        movement.y = 0f;

        bool isMoving = movement.x != 0;

        if (isMoving)
        {
            Vector2 direction = movement.x > 0 ? Vector2.right : Vector2.left;
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                Vector2 size = collider.size;
                float distance = Mathf.Abs(movement.x);
                int layerMask = ~0; // Check all layers
                RaycastHit2D hit = Physics2D.BoxCast(transform.position, size, 0f, direction, distance, layerMask);
                if (hit.collider == null)
                {
                    transform.Translate(movement);
                }
            }
            else
            {
                transform.Translate(movement);
            }

			// Improved footsteps system - only play when grounded
			bool isGrounded = IsGrounded();
			if (isGrounded)
			{
				if (!wasMovingLastFrame)
				{
					// Just started moving - play first footstep immediately
					PlayFootstepSound();
					lastFootstepTime = Time.time;
				}
				else if (Time.time - lastFootstepTime >= footstepsInterval)
				{
					// Time for next footstep
					PlayFootstepSound();
					lastFootstepTime = Time.time;
				}
			}
		}

		wasMovingLastFrame = isMoving;
    }

    public void KnockbackPlayer(Vector2 knockbackForce, int direction)
    {
		knockbackForce.x *= direction;
		rb.velocity = Vector2.zero;
		rb.angularVelocity = 0f;
        rb.AddForce(knockbackForce, ForceMode2D.Impulse);

		gameManager.coinCount = Mathf.Max(0, gameManager.coinCount - gameManager.enemyCollideCoinCost);
		DeathParticles();
		StartCoroutine(FlashRed(spriteRenderer));
	}

	public void DeathParticles()
	{
        Transform deathParticlesTransform = transform.Find("DeathParticles");
        if (deathParticlesTransform != null)
        {
            ParticleSystem ps = deathParticlesTransform.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
        }
	}

	private IEnumerator FlashRed(SpriteRenderer sr)
    {
        for (int i = 0; i < 3; i++)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
