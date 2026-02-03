using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Vector2 knockBackToSelf = new Vector2(2f, 2f);
    [SerializeField] private Vector2 knockBackToPlayer = new Vector2(2f, 2f);
    [SerializeField] private float knockBackDelayToSelf = 1.5f;

    private AudioSource damageAudio;
    private AudioSource damageCoinAudio;

    [Header("Shoot Audio Settings")]
	[SerializeField] private float damagePitchMin = 0.95f;
	[SerializeField] private float damagePitchMax = 1.05f;
    [SerializeField] private float damageCoinPitchMin = 0.95f;
    [SerializeField] private float damageCoinPitchMax = 1.05f;



    void Start()
    {
        damageAudio = GameObject.Find("DamageAudio").GetComponent<AudioSource>();
        damageCoinAudio = GameObject.Find("DamageCoinAudio").GetComponent<AudioSource>();
    }
    
    //private int direction = 1;
    public void Die()
    {
        Destroy(gameObject);
    }

    public void HitPlayer(Transform playerTransform)
    {
        int direction = GetDirection(playerTransform);
        FindObjectOfType<PlayerMovement>().KnockbackPlayer(knockBackToPlayer, direction);
        GetComponent<EnemyMovements>().KnockbackEnemy(knockBackToSelf, -direction, knockBackDelayToSelf);

        damageAudio.pitch = Random.Range(damagePitchMin, damagePitchMax);
        damageCoinAudio.pitch = Random.Range(damageCoinPitchMin, damageCoinPitchMax);
        damageAudio.PlayOneShot(damageAudio.clip);
        damageCoinAudio.PlayOneShot(damageCoinAudio.clip);
    }

    private int GetDirection(Transform playerTransform)
    {
        if(transform.position.x > playerTransform.position.x)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }
}
