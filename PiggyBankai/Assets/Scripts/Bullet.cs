using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 3f;
    private Vector2 screenBounds;

    void Start()
    {
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

    // void OnTriggerEnter2D(Collider2D collision)
    // {
    //     Debug.Log("Bullet hit via trigger: " + collision.gameObject.name);
    //     if (collision.CompareTag("Player"))
    //     {
    //         return;
    //     }
    //     Destroy(gameObject);
    // }
}
