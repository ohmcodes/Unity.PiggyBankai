using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [SerializeField] private TMP_Text yourRank;
    [SerializeField] private TMP_Text yourScore;
    [SerializeField] private TMP_Text currentDistance;
    [SerializeField] private TMP_Text coinConsumed;
    [SerializeField] private TMP_Text coinCollected;
    private GameManager gm;
    private LeaderboardManager lb;

    public bool fromPauseMenu = false;

    void Awake()
    {
        lb = FindObjectOfType<LeaderboardManager>();
        gm = FindObjectOfType<GameManager>();
    }

    public void SetFromPauseMenu(bool value)
    {
        fromPauseMenu = value;
    }

    public void ShowPlayerRank()
    {
        lb.GetPlayerRankDirect(rank => {
            if (rank > 0)
            {
                yourRank.text = "Your rank: #" + rank.ToString();
                //Debug.Log("Your current rank: " + rank);
                // Optionally highlight or show player's entry
            }
            else
            {
                yourRank.text = "Your rank: Unranked";
                //Debug.Log("You're not in the top leaderboard yet!");
            }
        });

        lb.GetPlayerScore(score => {
            yourScore.text = "Your Best:" + lb.FormatScore(score);
            //Debug.Log("Your current score: " + lb.FormatScore(score));
        });

        currentDistance.text = "Current:" + lb.FormatScore(gm.distanceWalked);
        coinConsumed.text = "Consumed:" + gm.consumedCoins.ToString();
        coinCollected.text = "Collected:" + gm.collectedCoins.ToString();
    }       
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Debug.Log("Resuming game from pause menu: " + fromPauseMenu);

        if(fromPauseMenu)
        {
            Time.timeScale = 1f;
            fromPauseMenu = false;
            Debug.Log("Game resumed.");
        }
        else
        {
            Debug.Log("Restarting game instead of resuming from pause menu.");
            StartCoroutine(gm.RestartGameAfterDelay(gm.restartDelay));
        }
    }

    public void GoBackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
