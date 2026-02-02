using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Vector2 knockBackToSelf = new Vector2(2f, 2f);
    [SerializeField] private Vector2 knockBackToPlayer = new Vector2(2f, 2f);
    [SerializeField] private float knockBackDelayToSelf = 1.5f;
    
    private int direction = 1;
    public void Die()
    {
        Destroy(gameObject);
    }

    public void HitPlayer(Transform playerTransform)
    {
        int direction = GetDirection(playerTransform);
        FindObjectOfType<PlayerMovement>().KnockbackPlayer(knockBackToPlayer, direction);
        GetComponent<EnemyMovements>().KnockbackEnemy(knockBackToSelf, -direction, knockBackDelayToSelf);
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
