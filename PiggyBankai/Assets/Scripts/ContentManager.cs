using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentManager : MonoBehaviour
{
    [SerializeField] private Entry entryPrefab;
    [SerializeField] private Transform entriesParent;

    private LeaderboardManager leaderboardManager;

    void Start()
    {
        if (leaderboardManager == null)
        {
            leaderboardManager = FindObjectOfType<LeaderboardManager>();
            //Debug.Log("LeaderboardManager found: " + (leaderboardManager != null));
            LoadLeaderboard();
        }
    }

    public void LoadLeaderboard()
    {
        //Debug.Log("LoadLeaderboard called");
        if (leaderboardManager == null)
        {
            Debug.LogError("LeaderboardManager is null!");
            return;
        }

        ClearEntries();

        leaderboardManager.GetTopLeaderboard(scores => {
            //Debug.Log("Received " + scores.Count + " scores");
            for (int i = 0; i < scores.Count; i++)
            {
                //Debug.Log("Score " + (i+1) + ": " + scores[i].playername + " - " + scores[i].score);
                if (entryPrefab != null && entriesParent != null)
                {
                    Entry entry = Instantiate(entryPrefab, entriesParent);
                    entry.SetEntry(i + 1, scores[i].playername, scores[i].score);
                    //Debug.Log("Entry instantiated for rank " + (i+1));
                }
                else
                {
                    Debug.LogError("entryPrefab or entriesParent is null!");
                }
            }
        });
    }

    private void ClearEntries()
    {
        if (entriesParent.childCount > 0)
        {
            foreach (Transform child in entriesParent)
            {
                Destroy(child.gameObject);
            }
        }
    }

    
}
