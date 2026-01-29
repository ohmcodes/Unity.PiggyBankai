using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private Animator animator;
	[SerializeField] private float unitsPerCoin = 5f;
	[SerializeField] private PlayerMovementState playerMovementState;
	public float wallJumpCooldown {get; set;}
    private Vector2 movement;
	private Vector2 initialPosition;
	private float maxX;
	private float coinProgress = 0f;
	private Vector2 screeBounds;
	private float playerHalfWidth;
	private float xPosLastFrame;
	private float distanceWalked = 0f;
	public CoinManager coinManager;

	public Transform bulletSpawnPoint;
	public GameObject bulletPrefab;
	public float bulletSpeed = 10f;

	public DistanceManager distanceManager;

    private void Start()
    {
        screeBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
		playerHalfWidth = spriteRenderer.bounds.extents.x;
		xPosLastFrame = transform.position.x;
		initialPosition = transform.position;
		maxX = transform.position.x;
    }
    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        //ClampMovement();
		float deltaX = transform.position.x - xPosLastFrame;
		if (deltaX > 0)
		{
			coinProgress += deltaX;
		}
		if (transform.position.x > maxX)
		{
			maxX = transform.position.x;
		}
		distanceWalked = maxX - initialPosition.x;
		distanceManager.distanceWalked = distanceWalked;
		FlipCharacterX();

		if (playerMovementState.currentState != PlayerMovementState.MovementState.Jump &&
		    playerMovementState.currentState != PlayerMovementState.MovementState.DoubleJump &&
		    playerMovementState.currentState != PlayerMovementState.MovementState.WallJump)
		{
			while (coinProgress >= unitsPerCoin)
			{
				coinManager.coinCount = Mathf.Max(0, coinManager.coinCount - 1);
				coinProgress -= unitsPerCoin;
			}
		}

		if(wallJumpCooldown > 0)
		{
			wallJumpCooldown -= Time.deltaTime;
		}

		if(Input.GetButtonDown("Fire1"))
		{
			GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
			Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
			Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
			rb.velocity = direction * bulletSpeed;
			coinManager.coinCount--;
		}
    
    }

	private void FlipCharacterX()
	{
		float input = Input.GetAxisRaw("Horizontal");
		if(input > 0 && (transform.position.x > xPosLastFrame))
		{
			spriteRenderer.flipX = false;
		}
		else if(input < 0 && (transform.position.x < xPosLastFrame))
		{
			spriteRenderer.flipX = true;
		}
		xPosLastFrame = transform.position.x;
	}

    private void ClampMovement()
    {
        float clampedX = Mathf.Clamp(transform.position.x, -screeBounds.x + playerHalfWidth, screeBounds.x - playerHalfWidth);
        Vector2 pos = transform.position;
        pos.x = clampedX;
        transform.position = pos;
    }

    private void HandleMovement()
    {
		if(wallJumpCooldown > 0f) {return;}

        float input = Input.GetAxisRaw("Horizontal");
        movement.x = input * speed * Time.deltaTime;
        movement.y = 0f;

        if (movement.x != 0)
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
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Coin"))
		{
			Destroy(collision.gameObject);
			coinManager.coinCount++;
		}
    }
}
