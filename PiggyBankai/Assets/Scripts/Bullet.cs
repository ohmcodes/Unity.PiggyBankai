using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 3f;
    [SerializeField] private GameObject coinPrefab;
    private GameManager gameManager;
    private Vector2 screenBounds;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    }

    void Awake()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // if (transform.position.x > screenBounds.x + 1f || transform.position.x < -screenBounds.x - 1f ||
        //     transform.position.y > screenBounds.y + 1f || transform.position.y < -screenBounds.y - 1f)
        // {
        //     Destroy(gameObject);
        // }
    }

    // void OnCollisionEnter2D(Collision2D collision)
    // {
    //     Debug.Log("Bullet hit via collision: " + collision.gameObject.name);
    //     if (collision.gameObject.CompareTag("Player"))
    //     {
    //         return;
    //     }
    //     Destroy(collision.gameObject);
    //     Destroy(gameObject);
    // }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("Bullet hit via trigger: " + collision.gameObject.name);
        if (collision.CompareTag("Enemy"))
        {
            StartCoroutine(FlashAndDestroy(collision));
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    private IEnumerator FlashAndDestroy(Collider2D collision)
    {
        SpriteRenderer spriteRenderer = collision.GetComponent<SpriteRenderer>();

        for (int i = 0; i < 3; i++)
        {
            if(spriteRenderer == null || spriteRenderer.IsDestroyed()) break;
            
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if(spriteRenderer == null || spriteRenderer.IsDestroyed()) break;
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }

        gameManager.SpawnCoins(collision, gameManager.shootCoinReward);
        
        Destroy(gameObject);
        if(collision != null && !collision.IsDestroyed())
        {
            Destroy(collision.gameObject);
        }
    }
}
