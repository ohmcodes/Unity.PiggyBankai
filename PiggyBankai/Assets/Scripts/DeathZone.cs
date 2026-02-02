using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public bool isDead = false;
    private Collider2D[] deathColliders; // Array of death zone colliders (found by tag)
    private Collider2D playerCollider;
    private GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        // Find all child GameObjects with tag "DeathZone" and get their colliders
        List<Collider2D> colliders = new List<Collider2D>();
        foreach (Transform child in transform)
        {
            if (child.CompareTag("DeathZone"))
            {
                Collider2D col = child.GetComponent<Collider2D>();
                if (col != null)
                {
                    colliders.Add(col);
                }
            }
        }
        deathColliders = colliders.ToArray();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerCollider = player.GetComponent<Collider2D>();
        }

        gm = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead && playerCollider != null && gm != null && deathColliders != null)
        {
            // Check if player is touching any death collider
            foreach (Collider2D deathCol in deathColliders)
            {
                if (deathCol != null && Physics2D.IsTouching(playerCollider, deathCol))
                {
                    TriggerDeath();
                    break;
                }
            }
        }

        // Check for coins touching death colliders
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        foreach (GameObject coin in coins)
        {
            Collider2D coinCol = coin.GetComponent<Collider2D>();
            if (coinCol != null)
            {
                foreach (Collider2D deathCol in deathColliders)
                {
                    if (deathCol != null && Physics2D.IsTouching(coinCol, deathCol))
                    {
                        Destroy(coin);
                        gm.coinCount += coin.GetComponent<Coin>().CoinValue;
                        gm.collectedCoins += coin.GetComponent<Coin>().CoinValue;
                        break;
                    }
                }
            }
        }

        // Check for enemies touching death colliders
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCol = enemy.GetComponent<Collider2D>();
            if (enemyCol != null)
            {
                foreach (Collider2D deathCol in deathColliders)
                {
                    if (deathCol != null && Physics2D.IsTouching(enemyCol, deathCol))
                    {
                        Destroy(enemy);
                        gm.SpawnCoins(enemyCol, gm.shootCoinReward);
                        break;
                    }
                }
            }
        }
    }

    private void TriggerDeath()
    {
        isDead = true;
        gm.isDead = true;
        PlayDeathParticles(playerCollider);
        // Add delay before respawning
        StartCoroutine(RespawnAfterDelay(gm));
    }

    private void PlayDeathParticles(Collider2D playerCollider)
    {
        Transform deathParticlesTransform = playerCollider.transform.Find("DeathParticles");
        if (deathParticlesTransform != null)
        {
            ParticleSystem ps = deathParticlesTransform.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                //Debug.Log("ParticleSystem Play called. IsPlaying: " + ps.isPlaying + ", ParticleCount: " + ps.particleCount);
            }
            else
            {
                //Debug.LogWarning("DeathParticles does not have ParticleSystem component");
            }
        }
        else
        {
            //Debug.LogWarning("DeathParticles child not found on player");
        }
    }

    private IEnumerator RespawnAfterDelay(GameManager gm)
    {
        yield return new WaitForSeconds(3f); // Delay for 3 seconds
        gm.RespawnPlayer();
        gm.isDead = false;
        isDead = false; // Reset after respawn
    }
}
