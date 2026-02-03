using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMovements : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float speed = 3f;
    [SerializeField] private int startDirection = 1;
    private GameManager gameManager;
    [SerializeField] private GameObject coinPrefab;



    private int currentDirection;
    private float halfWidth;
    private Vector2 movement;
    private float lastDeductTime;
    private float movementDelay;

    // Start is called before the first frame update
    private void Start()
    {
        halfWidth = spriteRenderer.bounds.extents.x;
        currentDirection = startDirection;
        spriteRenderer.flipX = startDirection == 1 ? false : true;
        lastDeductTime = Time.time - 2f; // Allow immediate deduction on first collision
        gameManager = FindObjectOfType<GameManager>();
    }

    private void FixedUpdate()
    {
        if (movementDelay > 0f)
        {
            movementDelay -= Time.fixedDeltaTime;
            return;
        }
        movement.x = speed * currentDirection;
        movement.y = rb.velocity.y;
        rb.velocity = movement;

       SetDirection();
    }

    private void SetDirection()
    {
        // Check for ledge or wall when moving right
        if (rb.velocity.x > 0)
        {
            // Check for ledge ahead (down from right edge)
            if (!Physics2D.Raycast(transform.position + Vector3.right * halfWidth, Vector2.down, 1f, LayerMask.GetMask("Ground")))
            {
                currentDirection *= -1;
                spriteRenderer.flipX = true;
            }
            // Check for wall to the right
            else if (Physics2D.Raycast(transform.position, Vector2.right, halfWidth + 0.1f, LayerMask.GetMask("Ground")))
            {
                currentDirection *= -1;
                spriteRenderer.flipX = true;
            }
        }
        // Check for ledge or wall when moving left
        else if (rb.velocity.x < 0)
        {
            // Check for ledge ahead (down from left edge)
            if (!Physics2D.Raycast(transform.position + Vector3.left * halfWidth, Vector2.down, 1f, LayerMask.GetMask("Ground")))
            {
                currentDirection *= -1;
                spriteRenderer.flipX = false;
            }
            // Check for wall to the left
            else if (Physics2D.Raycast(transform.position, Vector2.left, halfWidth + 0.1f, LayerMask.GetMask("Ground")))
            {
                currentDirection *= -1;
                spriteRenderer.flipX = false;
            }
        }

        Debug.DrawRay(transform.position + Vector3.right * halfWidth, Vector2.down * 1f, Color.blue);
        Debug.DrawRay(transform.position + Vector3.left * halfWidth, Vector2.down * 1f, Color.blue);
        Debug.DrawRay(transform.position, Vector2.right * (halfWidth + 0.1f), Color.red);
        Debug.DrawRay(transform.position, Vector2.left * (halfWidth + 0.1f), Color.red);
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
        // if (collision.gameObject.CompareTag("Player"))
        // {
        //     gameManager.coinCount = Mathf.Max(0, gameManager.coinCount - enemyCoinCost);

        //     Transform deathParticlesTransform = collision.transform.Find("DeathParticles");
        //     if (deathParticlesTransform != null)
        //     {
        //         ParticleSystem ps = deathParticlesTransform.GetComponent<ParticleSystem>();
        //         if (ps != null)
        //         {
        //             ps.Play();
        //         }
        //     }
        // }

        // if (collision.gameObject.CompareTag("Projectile"))
        // {
        //     Destroy(collision.gameObject);
        //     StartCoroutine(FlashAndDestroy());
        // }
        
    //}

    // private void OnCollisionStay2D(Collision2D collision)
    // {
    //     if (collision.gameObject.CompareTag("Player"))
    //     {
    //         if (Time.time - lastDeductTime >= 2f)
    //         {
    //             gameManager.coinCount = Mathf.Max(0, gameManager.coinCount - enemyCoinCost);
    //             lastDeductTime = Time.time;

    //             SpriteRenderer playerSr = collision.gameObject.GetComponent<SpriteRenderer>();
    //             if (playerSr != null)
    //             {
    //                 StartCoroutine(FlashRed(playerSr));
    //             }

    //             Transform deathParticlesTransform = collision.transform.Find("DeathParticles");
    //             if (deathParticlesTransform != null)
    //             {
    //                 ParticleSystem ps = deathParticlesTransform.GetComponent<ParticleSystem>();
    //                 if (ps != null)
    //                 {
    //                     ps.Play();
    //                 }
    //             }
    //         }
    //     }
    // }

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

    public void KnockbackEnemy(Vector2 knockbackForce, int direction, float delay)
    {
        movementDelay = delay;
		knockbackForce.x *= direction;
		rb.velocity = Vector2.zero;
		rb.angularVelocity = 0f;
        rb.AddForce(knockbackForce, ForceMode2D.Impulse);
    }
}
