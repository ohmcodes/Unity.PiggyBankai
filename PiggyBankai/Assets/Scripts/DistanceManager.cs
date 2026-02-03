using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistanceManager : MonoBehaviour
{
    //public float distanceWalked;
    [SerializeField] private Text distanceText;
    [SerializeField] private Text bestDistanceText;
    [SerializeField] private GameManager gameManager;



    // Start is called before the first frame update
    void Start()
    {
        bestDistanceText.text = "Best: " + gameManager.bestDistance.ToString("F2");
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.isDead) 
        { 
            distanceText.text = "Distance: " + gameManager.distanceWalked.ToString("F2");
        }
        
    }

    public void UpdateBestDistance(float newDistance)
    {
        bestDistanceText.text = "Best: " + newDistance.ToString("F2");
    }
}
