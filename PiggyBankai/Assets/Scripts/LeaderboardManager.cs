using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Runtime.InteropServices;

public class LeaderboardManager : MonoBehaviour
{
    public string endpoint = "https://leaderboard.ohmcodes.com"; // Base endpoint
    private string playerId;

    // JavaScript interop for WebGL
    [DllImport("__Internal")]
    private static extern string GetBrowserId();

    [Serializable]
    public class ScoreEntry
    {
        public string id;
        public string playername;
        public float score;
    }

    [Serializable]
    public class LeaderboardData
    {
        public List<ScoreEntry> scores;
    }

    [Serializable]
    public class ScoreArrayWrapper
    {
        public ScoreEntry[] items;
    }

    [Serializable]
    public class PlayerRankData
    {
        public int rank;
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

    public void UpdatePlayerName(string newName)
    {
        StartCoroutine(UpdatePlayerNameCoroutine(newName));
    }

    private IEnumerator SubmitScoreCoroutine(float score)
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Anonymous");
        ScoreEntry entry = new ScoreEntry { id = playerId, playername = playerName, score = score };
        string json = JsonUtility.ToJson(entry);
        //Debug.Log("Submitting score - ID: " + playerId + ", Name: " + playerName + ", Score: " + score + ", JSON: " + json);

        UnityWebRequest request = new UnityWebRequest(endpoint + "/score", "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log("Score submitted successfully");
        }
        else
        {
            //Debug.LogError("Error submitting score: " + request.error);
            //Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }

    private IEnumerator UpdatePlayerNameCoroutine(string newName)
    {
        // Create a simple JSON object with the new name
        string json = "{\"playername\":\"" + newName + "\"}";
        Debug.Log("Updating player name - ID: " + playerId + ", New Name: " + newName + ", JSON: " + json);

        UnityWebRequest request = new UnityWebRequest(endpoint + "/player/" + playerId + "/name", "PUT");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Player name updated successfully");
        }
        else
        {
            Debug.LogError("Error updating player name: " + request.error);
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

    public void GetPlayerRankDirect(System.Action<int> callback)
    {
        StartCoroutine(GetPlayerRankDirectCoroutine(callback));
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
            //Debug.Log("Received player score JSON: " + json);
            
            // Try parsing as object first
            try
            {
                ScoreEntry entry = JsonUtility.FromJson<ScoreEntry>(json);
                callback?.Invoke(entry.score);
            }
            catch
            {
                // If it's just a number, parse directly
                if (float.TryParse(json, out float score))
                {
                    callback?.Invoke(score);
                }
                else
                {
                    //Debug.LogError("Failed to parse player score: " + json);
                    callback?.Invoke(0f);
                }
            }
        }
        else
        {
            //Debug.LogError("Error getting player score: " + request.error);
            //Debug.LogError("Response: " + request.downloadHandler.text);
            callback?.Invoke(0f);
        }
    }

    private IEnumerator GetPlayerRankDirectCoroutine(System.Action<int> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(endpoint + "/rank/" + playerId);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            //Debug.Log("Received rank JSON: " + json);
            
            try
            {
                // Try to parse as object first
                PlayerRankData rankData = JsonUtility.FromJson<PlayerRankData>(json);
                callback?.Invoke(rankData.rank);
            }
            catch
            {
                // If it's just a number, parse directly
                if (int.TryParse(json, out int rank))
                {
                    callback?.Invoke(rank);
                }
                else
                {
                    //Debug.LogError("Failed to parse player rank: " + json);
                    callback?.Invoke(-1);
                }
            }
        }
        else
        {
            //Debug.LogError("Error getting player rank: " + request.error);
            //Debug.LogError("Response: " + request.downloadHandler.text);
            callback?.Invoke(-1);
        }
    }

    private IEnumerator GetLeaderboardCoroutine(System.Action<List<ScoreEntry>> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(endpoint + "/list");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            //Debug.Log("Received leaderboard JSON: " + json);
            
            LeaderboardData data = new LeaderboardData();
            
            // Check if JSON starts with [ (array) or { (object)
            if (json.TrimStart().StartsWith("["))
            {
                // Server returns direct array - wrap it for parsing
                string wrappedJson = "{\"items\":" + json + "}";
                ScoreArrayWrapper wrapper = JsonUtility.FromJson<ScoreArrayWrapper>(wrappedJson);
                data.scores = new List<ScoreEntry>(wrapper.items);
            }
            else
            {
                // Server returns object with scores field
                data = JsonUtility.FromJson<LeaderboardData>(json);
            }
            
            callback?.Invoke(data.scores);
        }
        else
        {
            //Debug.LogError("Error getting leaderboard: " + request.error);
            //Debug.LogError("Response: " + request.downloadHandler.text);
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
            //Debug.Log("Received JSON: " + json);
            
            LeaderboardData data = new LeaderboardData();
            
            // Check if JSON starts with [ (array) or { (object)
            if (json.TrimStart().StartsWith("["))
            {
                // Server returns direct array - wrap it for parsing
                string wrappedJson = "{\"items\":" + json + "}";
                ScoreArrayWrapper wrapper = JsonUtility.FromJson<ScoreArrayWrapper>(wrappedJson);
                data.scores = new List<ScoreEntry>(wrapper.items);
            }
            else
            {
                // Server returns object with scores field
                data = JsonUtility.FromJson<LeaderboardData>(json);
            }
            
            // Assume server returns top 10 sorted, but limit if needed
            List<ScoreEntry> topScores = data.scores.GetRange(0, Mathf.Min(limit, data.scores.Count));
            
            callback?.Invoke(topScores);
        }
        else
        {
            //Debug.LogError("Error getting leaderboard: " + request.error);
            //Debug.LogError("Response: " + request.downloadHandler.text);
            callback?.Invoke(new List<ScoreEntry>());
        }
    }

    public string FormatScore(float score)
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


/*
leaderboardManager.GetPlayerRank(rank => {
    if (rank > 0) {
        //Debug.Log("Your rank: " + rank);
    } else {
        //Debug.Log("You're not on the leaderboard yet!");
    }
});

// Get your own rank
leaderboardManager.GetPlayerRank(rank => {
    //Debug.Log("My rank: " + rank);
});

// Get rank for a specific ID (e.g., from another player)
leaderboardManager.GetRankForId("some-unique-id", rank => {
    //Debug.Log("Their rank: " + rank);
});

// Get top 10 (default)
leaderboardManager.GetTopLeaderboard(scores => {
    foreach (var score in scores) {
        //Debug.Log($"{score.id}: {score.score}");
    }
});

// Get top 5
leaderboardManager.GetTopLeaderboard(scores => {
    // Handle top 5 scores
}, 5);
*/