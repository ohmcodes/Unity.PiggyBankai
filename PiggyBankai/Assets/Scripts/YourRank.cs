using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class YourRank : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text scoreText;

    private LeaderboardManager leaderboardManager;

    void Awake()
    {
        leaderboardManager = FindObjectOfType<LeaderboardManager>();
    }

    public void ShowPlayerRank()
    {
        if (leaderboardManager != null)
        {
            // Get rank directly from server (more efficient)
            leaderboardManager.GetPlayerRankDirect(rank => {
                if (rank > 0)
                {
                    rankText.text = "Your rank:\n#" + rank.ToString();
                    //Debug.Log("Your current rank: " + rank);
                    // Optionally highlight or show player's entry
                }
                else
                {
                    rankText.text = "Your rank:\nUnranked";
                    //Debug.Log("You're not in the top leaderboard yet!");
                }
            });

            // Get player score separately
            leaderboardManager.GetPlayerScore(score => {
                scoreText.text = "Your Best:\n" + FormatScore(score);
                //Debug.Log("Your current score: " + FormatScore(score));
            });
        }
    }

    private string FormatScore(float score)
    {
        if (score >= 1000000000000000000000f) // 10^21
            return (score / 1000000000000000000000f).ToString("F2") + " " + "Si";
        else if (score >= 1000000000000000000f) // 10^18
            return (score / 1000000000000000000f).ToString("F2") + " " + "Qi";
        else if (score >= 1000000000000000f) // 10^15
            return (score / 1000000000000000f).ToString("F2") + " " + "Tr";
        else if (score >= 1000000000000f) // 10^12
            return (score / 1000000000000f).ToString("F2") + " " + "T";
        else if (score >= 1000000000f) // 10^9
            return (score / 1000000000f).ToString("F2") + " " + "B";
        else if (score >= 1000000f) // 10^6
            return (score / 1000000f).ToString("F2") + " " + "M";
        else if (score >= 1000f) // 10^3
            return (score / 1000f).ToString("F2") + " " + "K";
        else
            return score.ToString("F2");
    }
}
