using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class LeaderboardManager : MonoBehaviour
{
    public string endpoint = "https://leaderboard.ohmcodes.com"; // Base endpoint
    private string playerId;

    [Serializable]
    public class ScoreEntry
    {
        public string id;
        public float score;
    }

    [Serializable]
    public class LeaderboardData
    {
        public List<ScoreEntry> scores;
    }

    void Start()
    {
        // Generate or get player ID
        playerId = GetPlayerId();
    }

    private string GetPlayerId()
    {
        // Try to get from PlayerPrefs first
        string id = PlayerPrefs.GetString("PlayerId", "");
        if (string.IsNullOrEmpty(id))
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
                // For WebGL, use JavaScript to get/create ID in localStorage
                id = GetBrowserId();
            #else
                // For other platforms, use deviceUniqueIdentifier
                id = SystemInfo.deviceUniqueIdentifier;
                if (string.IsNullOrEmpty(id) || id == "00000000000000000000000000000000")
                {
                    // Fallback for web or if deviceUniqueIdentifier fails
                    id = Guid.NewGuid().ToString();
                }
            #endif
            PlayerPrefs.SetString("PlayerId", id);
            PlayerPrefs.Save();
        }
        return id;
    }

    public string GetCurrentPlayerId()
    {
        return playerId;
    }

    public void SubmitScore(float score)
    {
        StartCoroutine(SubmitScoreCoroutine(score));
    }

    private IEnumerator SubmitScoreCoroutine(float score)
    {
        ScoreEntry entry = new ScoreEntry { id = playerId, score = score };
        string json = JsonUtility.ToJson(entry);
        Debug.Log("Submitting score - ID: " + playerId + ", Score: " + score + ", JSON: " + json);

        UnityWebRequest request = new UnityWebRequest(endpoint + "/score", "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Score submitted successfully");
        }
        else
        {
            Debug.LogError("Error submitting score: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }

    public void GetLeaderboard(System.Action<List<ScoreEntry>> callback)
    {
        StartCoroutine(GetLeaderboardCoroutine(callback));
    }

    public void GetTopLeaderboard(System.Action<List<ScoreEntry>> callback, int limit = 10)
    {
        StartCoroutine(GetTopLeaderboardCoroutine(callback, limit));
    }

    public void GetPlayerRank(System.Action<int> callback)
    {
        GetRankForId(playerId, callback);
    }

    public void GetPlayerScore(System.Action<float> callback)
    {
        StartCoroutine(GetPlayerScoreCoroutine(callback));
    }

    public void GetRankForId(string id, System.Action<int> callback)
    {
        GetLeaderboard(scores => {
            // Sort scores descending (highest first)
            scores.Sort((a, b) => b.score.CompareTo(a.score));
            
            // Find player's position (1-based index)
            int rank = -1;
            for (int i = 0; i < scores.Count; i++)
            {
                if (scores[i].id == id)
                {
                    rank = i + 1; // 1-based ranking
                    break;
                }
            }
            
            callback?.Invoke(rank);
        });
    }

    private IEnumerator GetPlayerScoreCoroutine(System.Action<float> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(endpoint + "/score/" + playerId);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            // Assume it returns {"score": 15.73} or similar
            ScoreEntry entry = JsonUtility.FromJson<ScoreEntry>(json);
            callback?.Invoke(entry.score);
        }
        else
        {
            Debug.LogError("Error getting player score: " + request.error);
            callback?.Invoke(0f);
        }
    }

    private IEnumerator GetLeaderboardCoroutine(System.Action<List<ScoreEntry>> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(endpoint + "/top10");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);
            callback?.Invoke(data.scores);
        }
        else
        {
            Debug.LogError("Error getting leaderboard: " + request.error);
            callback?.Invoke(new List<ScoreEntry>());
        }
    }

    private IEnumerator GetTopLeaderboardCoroutine(System.Action<List<ScoreEntry>> callback, int limit)
    {
        UnityWebRequest request = UnityWebRequest.Get(endpoint + "/top10");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);
            
            // Assume server returns top 10 sorted, but limit if needed
            List<ScoreEntry> topScores = data.scores.GetRange(0, Mathf.Min(limit, data.scores.Count));
            
            callback?.Invoke(topScores);
        }
        else
        {
            Debug.LogError("Error getting leaderboard: " + request.error);
            callback?.Invoke(new List<ScoreEntry>());
        }
    }
}


/*
leaderboardManager.GetPlayerRank(rank => {
    if (rank > 0) {
        Debug.Log("Your rank: " + rank);
    } else {
        Debug.Log("You're not on the leaderboard yet!");
    }
});

// Get your own rank
leaderboardManager.GetPlayerRank(rank => {
    Debug.Log("My rank: " + rank);
});

// Get rank for a specific ID (e.g., from another player)
leaderboardManager.GetRankForId("some-unique-id", rank => {
    Debug.Log("Their rank: " + rank);
});

// Get top 10 (default)
leaderboardManager.GetTopLeaderboard(scores => {
    foreach (var score in scores) {
        Debug.Log($"{score.id}: {score.score}");
    }
});

// Get top 5
leaderboardManager.GetTopLeaderboard(scores => {
    // Handle top 5 scores
}, 5);
*/