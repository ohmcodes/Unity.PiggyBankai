using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private FloorManager floorManager;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private CoinManager coinManager;
    [SerializeField] private DistanceManager distanceManager;
    [SerializeField] private int deathCoinReduction = 1;
    public float restartDelay = 2f;
    [SerializeField] private float currentBestDistance = 0f;
    [SerializeField] private TMPro.TMP_Text playernameText;


    private LeaderboardManager lb;

    private AudioSource deathAudio;
    

    public int coinCount = 100;
    public float distanceWalked = 0f;
    public float bestDistance = 0f;
    // public bool canMove = true;
    // public bool canJump = true;
    // public bool canDoubleJump = true;
    // public bool canWallJump = true;
    // public bool canShoot = true;
    public bool isDead = false;
    public int collectedCoins = 0;
    public int consumedCoins = 0;
    public int stompCoinReward = 5;
    public int shootCoinReward = 5;
    public int enemyCollideCoinCost = 5;

    private AudioSource bestDistanceAudio;
    private bool playedBestDistanceAudio = false;

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Pause pauseManager;

    private bool isGameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        lb = FindObjectOfType<LeaderboardManager>();
        bestDistanceAudio = GameObject.Find("BestDistanceAudio").GetComponent<AudioSource>();
        deathAudio = GameObject.Find("LoseGameAudio").GetComponent<AudioSource>();

        // Wait for leaderboard manager to initialize player ID
        //StartCoroutine(LoadPersistentData());

        LoadCurrentBest();

        playernameText.text = PlayerPrefs.GetString("PlayerName", "Guest");
    }

    private void LoadCurrentBest()
    {
        string playerKey = GetPlayerPrefsKey();
        lb.GetPlayerScore(serverScore => {
            currentBestDistance = Mathf.Max(currentBestDistance, serverScore);
            bestDistance = currentBestDistance;
            // Update DistanceManager UI
            distanceManager.UpdateBestDistance(currentBestDistance);
            // Optionally save back to PlayerPrefs
            PlayerPrefs.SetFloat(playerKey + "_CurrentBestDistance", currentBestDistance);
            PlayerPrefs.Save();
            //Debug.Log("Loaded persistent data - Best Distance: " + currentBestDistance + ", Coin Count: " + coinCount);
        });
    }

    // private IEnumerator LoadPersistentData()
    // {
    //     string playerKey = GetPlayerPrefsKey();
    //     // Wait a frame for lb to initialize
    //     yield return null;
    //     // Load from PlayerPrefs first
    //     currentBestDistance = PlayerPrefs.GetFloat(playerKey + "_CurrentBestDistance", 0f);
    // }

    private string GetPlayerPrefsKey()
    {
        // Use player ID for unique keys per player
        if (lb != null)
        {
            // Access the playerId from LeaderboardManager
            return "Player_" + lb.GetCurrentPlayerId();
        }
        return "Player_Default";
    }

    // Update is called once per frame
    void Update()
    {
        GameOver();

        BestDistanceReachedCheck();
    }

    private void BestDistanceReachedCheck()
    {
        if (distanceWalked > bestDistance)
        {
            if (!playedBestDistanceAudio)
            {
                bestDistanceAudio.Play();
                playedBestDistanceAudio = true;
            }
        }
    }

    private void GameOver()
    {
        if(isGameOver) { return; }

        if (coinCount == 0)
        {
            // Disable all player movements
            DisablePlayerMovements();
            
            // Save persistent data before restarting
            //SavePersistentData();

            if (deathAudio != null && deathAudio.clip != null)
            {
                deathAudio.Play();
            }
            if(pauseMenu != null)
            {
                pauseManager.ShowPlayerRank();
                pauseMenu.SetActive(true);
            }

            isDead = false;
            isGameOver = true;
        }
    }

    private void DisablePlayerMovements()
    {
        if (player != null)
        {
            // Disable movement scripts
            var playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null) playerMovement.enabled = false;

            var playerJump = player.GetComponent<PlayerJump>();
            if (playerJump != null) playerJump.enabled = false;

            var playerMovementState = player.GetComponent<PlayerMovementState>();
            if (playerMovementState != null) playerMovementState.enabled = false;

            // Disable Rigidbody if needed to stop physics
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true; // Or disable gravity, etc.
            }

            //Debug.Log("Player movements disabled due to zero coins");
        }
    }

    public IEnumerator RestartGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (distanceWalked > currentBestDistance)
        {
            currentBestDistance = distanceWalked;
            distanceManager.UpdateBestDistance(currentBestDistance);
            lb.SubmitScore(currentBestDistance);
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // private void SavePersistentData()
    // {
    //     string playerKey = GetPlayerPrefsKey();
    //     PlayerPrefs.SetFloat(playerKey + "_CurrentBestDistance", currentBestDistance);
    //     PlayerPrefs.Save();
    // }

    public Transform GetPreviousPlayerCheckpoint()
    {
        if (floorManager != null)
        {
            Transform prevCheckpoint = floorManager.GetPreviousPlayerCheckpoint();
            if (prevCheckpoint != null)
            {
                return prevCheckpoint;
            }
            else
            {
                // If no previous checkpoint (e.g., first floor), use the current floor's checkpoint
                return floorManager.playerCheckpoint;
            }
        }
        else
        {
            //Debug.LogWarning("FloorManager not set");
            return null;
        }
    }

    // Method to respawn the player to the previous floor's checkpoint
    public void RespawnPlayer()
    {
        Transform checkpoint = floorManager.playerCheckpoint;
        if (player != null)
        {
            if (checkpoint != null)
            {
                player.position = checkpoint.position;
                //Debug.Log("Player respawned to checkpoint: " + checkpoint.position);

                UpdateCurrentFloorAndCamera(checkpoint);
                // Now currentFloor is the respawn floor
                if (floorManager.spawnedFloors.IndexOf(floorManager.currentFloor) != 0)
                {
                    GameObject checkpointFlag = floorManager.currentFloor.transform.Find("CheckPoint").gameObject;
                    if (checkpointFlag != null)
                    {
                        checkpointFlag.SetActive(true);
                    }
                }
            }
            else
            {
                // Fallback to origin if no checkpoint
                player.position = Vector3.zero;
                //Debug.Log("No checkpoint found, respawned to origin");
               
            }
            
            // Reduce coins as penalty
            coinCount = Mathf.Max(0, coinCount - deathCoinReduction);
            // Update best distance if applicable
            if (distanceWalked > currentBestDistance)
            {
                currentBestDistance = distanceWalked;
                distanceManager.UpdateBestDistance(currentBestDistance);
                lb.SubmitScore(currentBestDistance);
            }
        }
        else
        {
            //Debug.LogWarning("Player not set in GameManager");
        }

        

    }

    // Method to update the current floor and camera boundaries after respawn
    private void UpdateCurrentFloorAndCamera(Transform checkpoint)
    {
        if (floorManager != null)
        {
            // Find which floor contains this checkpoint
            foreach (GameObject floor in floorManager.spawnedFloors)
            {
                Transform floorCheckpoint = floor.transform.Find("PlayerCheckpoint");
                if (floorCheckpoint == checkpoint)
                {
                    floorManager.currentFloor = floor;
                    floorManager.UpdateCameraConfiner();
                    //Debug.Log("Current floor updated to respawn floor, camera boundaries updated");
                    break;
                }
            }
        }
    }

    public void SpawnCoins(Collision2D collision, int count)
    {
        if(!collision.gameObject.CompareTag("Enemy")) { return; }

        for (int i = 0; i < count; i++)
        {
            GameObject coin = Instantiate(coinPrefab, collision.transform.position, Quaternion.identity);
            Rigidbody2D rb = coin.AddComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 2f;
                rb.mass = 0.05f;
                Vector2 impulse = Random.insideUnitCircle.normalized * 1f;
                rb.AddForce(impulse, ForceMode2D.Impulse);
            }
        }
    }

    public void SpawnCoins(Collider2D collision, int count)
    {
        if(collision==null || !collision.CompareTag("Enemy")) { return; }

        for (int i = 0; i < count; i++)
        {
            GameObject coin = Instantiate(coinPrefab, collision.transform.position, Quaternion.identity);
            Rigidbody2D rb = coin.AddComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 2f;
                rb.mass = 0.05f;
                Vector2 impulse = Random.insideUnitCircle.normalized * 1f;
                rb.AddForce(impulse, ForceMode2D.Impulse);
            }
        }
    }
}
