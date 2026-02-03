using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float bounceForce = 6f;
    [SerializeField] private float bounceForceMultiplier = 0.8f;
    private GameManager gameManager;

    private float halfHeight;

    private AudioSource stompAudio;

    [Header("Shoot Audio Settings")]
	[SerializeField] private float stompPitchMin = 0.95f;
	[SerializeField] private float stompPitchMax = 1.05f;


    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        halfHeight = spriteRenderer.bounds.extents.y;
        stompAudio = GameObject.Find("StompAudio").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        float halfWidth = spriteRenderer.bounds.extents.x;
        Vector2 leftPos = transform.position - Vector3.right * halfWidth * 0.3f;
        Vector2 middlePos = transform.position;
        Vector2 rightPos = transform.position + Vector3.right * halfWidth * 0.7f;

        Debug.DrawRay(leftPos, Vector2.down * (halfHeight + 0.1f), Color.red);
        Debug.DrawRay(middlePos, Vector2.down * (halfHeight + 0.1f), Color.red);
        Debug.DrawRay(rightPos, Vector2.down * (halfHeight + 0.1f), Color.red);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy"))
        {
            //print("Player collided with Enemy");
            CollideWithEnemy(collision);
        }
    }

    private void CollideWithEnemy(Collision2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        float halfWidth = spriteRenderer.bounds.extents.x;
        Vector2 leftPos = transform.position - Vector3.right * halfWidth * 0.3f;
        Vector2 middlePos = transform.position;
        Vector2 rightPos = transform.position + Vector3.right * halfWidth * 0.7f;

        bool stomped = Physics2D.Raycast(leftPos, Vector2.down, halfHeight + 0.1f, LayerMask.GetMask("Enemy")) ||
                       Physics2D.Raycast(middlePos, Vector2.down, halfHeight + 0.1f, LayerMask.GetMask("Enemy")) ||
                       Physics2D.Raycast(rightPos, Vector2.down, halfHeight + 0.1f, LayerMask.GetMask("Enemy"));

        //bool stomped = Physics2D.Raycast(transform.position, Vector2.down, halfHeight + 0.1f, LayerMask.GetMask("Enemy"));
        if (stomped)
        {
            //Debug.Log("Enemy Stomped by Player");
            Vector2 velocity = rb.velocity;
            velocity.y = 0f;
            rb.velocity = velocity;
            //rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);

            float force = bounceForce;
            // if(Input.GetButton("Jump"))
            // {
            //     print("High Bounce!");
                
            // }
            force *= bounceForceMultiplier;
            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);

            gameManager.SpawnCoins(collision, gameManager.stompCoinReward);
            enemy.Die();

            stompAudio.pitch = Random.Range(stompPitchMin, stompPitchMax);
			stompAudio.PlayOneShot(stompAudio.clip);
        }
        else
        {
            enemy.HitPlayer(transform);
        }
    }
}
