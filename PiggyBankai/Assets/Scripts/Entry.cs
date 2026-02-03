using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Entry : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text playernameText;
    public TMP_Text scoreText;
    
    private void Reset()
    {
        rankText.text = "";
        playernameText.text = "";
        scoreText.text = "";
    }

    public void SetEntry(int rank, string playerName, float score)
    {
        rankText.text = "#" + rank.ToString();
        playernameText.text = playerName;
        scoreText.text = FormatScore(score);
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
