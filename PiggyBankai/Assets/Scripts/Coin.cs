using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private GameManager gameManager;
    public int CoinValue = 1;
    private Rigidbody2D rb;
    private GameObject player;
    private float timer = 0f;
    private bool magnetActive = false;
    public float magnetSpeed = 5f;
    [SerializeField] private string[] excludeLayerNames = { "Ground", "Enemy"};

    private AudioSource coinAudio;
    private AudioSource eatAudio;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        coinAudio = GameObject.Find("CoinAudio").GetComponent<AudioSource>();
        eatAudio = GameObject.Find("EatAudio").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(rb!=null)
        {
            timer += Time.deltaTime;
            if (timer > 3f && !magnetActive)
            {
                rb.excludeLayers = LayerMask.GetMask(excludeLayerNames);
                magnetActive = true;
            }
            if (magnetActive && player != null)
            {
                rb.MovePosition(Vector2.MoveTowards(transform.position, player.transform.position, magnetSpeed * Time.deltaTime));
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        // {
        //     rb.velocity = new Vector2(rb.velocity.x, 0);
        //     rb.gravityScale = 0;
        // }

        if(collision.gameObject.CompareTag("Player"))
        {
            if(eatAudio != null && eatAudio.clip != null)
            {
                eatAudio.Play();
                coinAudio.Play();
            }
            Destroy(gameObject);
            gameManager.coinCount += CoinValue;
            gameManager.collectedCoins += CoinValue;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
		{
            if(eatAudio != null && eatAudio.clip != null)
            {
                eatAudio.Play();
                coinAudio.Play();
            }
            
			Destroy(gameObject);
            gameManager.coinCount += CoinValue;
            gameManager.collectedCoins += CoinValue;
		}
    }
}
