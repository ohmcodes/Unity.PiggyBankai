using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FloorManager : MonoBehaviour
{
    [SerializeField] private GameObject[] floorPrefabs;
    [SerializeField] private Transform player;
    [SerializeField] private CinemachineStateDrivenCamera stateDrivenCamera;
    public Transform playerCheckpoint;
    public GameObject currentFloor; 
    public List<GameObject> spawnedFloors = new List<GameObject>();

    private Vector3 lastEndPosition = Vector3.zero;
    private GameObject lastSpawnedPrefab; 
    private Transform previousPlayerCheckpoint;
    

    void Start()
    {
        // Spawn initial 3 random floors
        SpawnFloor();
        currentFloor = spawnedFloors[0];
        SpawnFloor();
        SpawnFloor();
        // Set initial camera confiner
        UpdateCameraConfiner();
        playerCheckpoint = currentFloor.transform.Find("PlayerCheckpoint");
        previousPlayerCheckpoint = playerCheckpoint;
    }

    void Update()
    {
        //Debug.Log("Current Floor: " + (currentFloor != null ? currentFloor.name : "None"));

        // Check if player has reached the SpawnEndLocation of the current floor
        if (player != null && currentFloor != null)
        {
            Transform startLoc = currentFloor.transform.Find("SpawnStartLocation");
            Transform endLoc = currentFloor.transform.Find("SpawnEndLocation");
            if (endLoc != null && player.position.x > endLoc.position.x)
            {
                int currentIndex = spawnedFloors.IndexOf(currentFloor);
                if (currentIndex == spawnedFloors.Count - 1)
                {
                    // Reached the end of the last floor, spawn a new one
                    SpawnFloor();
                    currentFloor = spawnedFloors[spawnedFloors.Count - 1];
                }
                else
                {
                    // Move to the next existing floor
                    currentFloor = spawnedFloors[currentIndex + 1];
                }
                // Update camera confiner to the new current floor's boundaries
                UpdateCameraConfiner();
                SetCheckpoints();
                // Ensure there are always 3 floors ahead
                int floorsAhead = spawnedFloors.Count - spawnedFloors.IndexOf(currentFloor) - 1;
                while (floorsAhead < 3)
                {
                    SpawnFloor();
                    floorsAhead++;
                }
                // Optional: Remove old floors to save memory
                CleanUpOldFloors();
            }
            else if (endLoc != null && player.position.x < startLoc.position.x)
            {
                int currentIndex = spawnedFloors.IndexOf(currentFloor);
                if (currentIndex > 0)
                {
                    // Player went back, set to previous floor
                    currentFloor = spawnedFloors[currentIndex - 1];
                    // Update camera confiner to the new current floor's boundaries
                    UpdateCameraConfiner();
                    SetCheckpoints();
                }
            }
        }
    }

    private void SetCheckpoints()
    {
        // Update player checkpoint to the current floor's checkpoint
        if (currentFloor != null)
        {
            Transform newCheckpoint = currentFloor.transform.Find("PlayerCheckpoint");
            if (newCheckpoint != null)
            {
                previousPlayerCheckpoint = playerCheckpoint; // Store the old checkpoint
                int newIndex = spawnedFloors.IndexOf(currentFloor);
                if (newIndex == 0)
                {
                    playerCheckpoint = newCheckpoint;
                }
                else
                {
                    Transform prevFloorCheckpoint = spawnedFloors[newIndex - 1].transform.Find("PlayerCheckpoint");
                    if (prevFloorCheckpoint != null)
                    {
                        playerCheckpoint = prevFloorCheckpoint;
                    }
                    else
                    {
                        // Fallback to current floor's checkpoint
                        playerCheckpoint = newCheckpoint;
                        Debug.LogWarning("PlayerCheckpoint not found in previous floor, using current floor's");
                    }
                }
                //Debug.Log("Player checkpoint updated to current floor's PlayerCheckpoint");
            }
            else
            {
                //Debug.LogWarning("PlayerCheckpoint not found in current floor");
            }
        }
    }

    void SpawnFloor()
    {
        // Select a random floor prefab from the array, avoiding the last spawned one
        if (floorPrefabs.Length > 0)
        {
            GameObject selectedPrefab;
            do
            {
                selectedPrefab = floorPrefabs[Random.Range(0, floorPrefabs.Length)];
            } while (selectedPrefab == lastSpawnedPrefab && floorPrefabs.Length > 1);
            
            lastSpawnedPrefab = selectedPrefab;
            
            GameObject floor;
            if (spawnedFloors.Count == 0)
            {
                // First floor: spawn at lastEndPosition (0,0)
                floor = Instantiate(selectedPrefab, lastEndPosition, Quaternion.identity);
            }
            else
            {
                // Subsequent floors: spawn at 0,0 then move to align
                floor = Instantiate(selectedPrefab, Vector3.zero, Quaternion.identity);
            }
            spawnedFloors.Add(floor);

            GameObject checkpointFlag = floor.transform.Find("CheckPoint").gameObject;
            checkpointFlag.SetActive(false);
            
            // Find the SpawnStartLocation and SpawnEndLocation children
            Transform startLoc = floor.transform.Find("SpawnStartLocation");
            Transform endLoc = floor.transform.Find("SpawnEndLocation");
            if (startLoc != null && endLoc != null)
            {
                if (spawnedFloors.Count == 1)
                {
                    // First floor: just set next spawn position to SpawnEndLocation
                    lastEndPosition = endLoc.position;
                    //Debug.Log($"First floor spawned at {lastEndPosition}, SpawnEndLocation at {endLoc.position}");
                }
                else
                {
                    // Subsequent floors: move so SpawnStartLocation aligns with lastEndPosition
                    Vector3 offset = lastEndPosition - startLoc.position;
                    floor.transform.position += offset;
                    
                    //Debug.Log($"Spawned floor, moved by offset {offset}, SpawnStartLocation at {startLoc.position}, SpawnEndLocation at {endLoc.position}");
                    
                    // Set next spawn position to the SpawnEndLocation
                    lastEndPosition = endLoc.position;
                    
                    //Debug.Log($"Next lastEndPosition set to: {lastEndPosition}");
                }
            }
            else
            {
                //Debug.LogWarning("SpawnStartLocation or SpawnEndLocation not found in prefab, using fallback width");
            }
        }
    }

    void CleanUpOldFloors()
    {
        // Remove old floors to keep only the last 10 floors
        while (spawnedFloors.Count > 10)
        {
            GameObject oldFloor = spawnedFloors[0];
            spawnedFloors.RemoveAt(0);
            if (currentFloor == oldFloor)
            {
                // If current floor is being removed, set to the next one if available
                currentFloor = spawnedFloors.Count > 0 ? spawnedFloors[0] : null;
            }
            Destroy(oldFloor);
        }
        
        // Also remove floors that are far behind the player (optional, for extra cleanup)
        // float cleanupDistance = 30f; // Adjust as needed
        // for (int i = spawnedFloors.Count - 1; i >= 0; i--)
        // {
        //     if (spawnedFloors[i].transform.position.x < player.position.x - cleanupDistance)
        //     {
        //         Destroy(spawnedFloors[i]);
        //         spawnedFloors.RemoveAt(i);
        //     }
        // }
    }

    // Method to dynamically set the camera confiner
    public void SetCameraConfiner(Collider2D newBoundingShape)
    {
        if (stateDrivenCamera != null)
        {
            // Find the "RunCamera" through the State-Driven Camera's children
            CinemachineVirtualCamera runCam = null;
            foreach (Transform child in stateDrivenCamera.transform)
            {
                if (child.name == "RunCamera")
                {
                    runCam = child.GetComponent<CinemachineVirtualCamera>();
                    break;
                }
            }
            if (runCam != null)
            {
                CinemachineConfiner confiner = runCam.GetComponent<CinemachineConfiner>();
                if (confiner != null)
                {
                    confiner.m_BoundingShape2D = newBoundingShape;
                    // Enable damping for smoother transitions
                    confiner.m_Damping = 1.5f; // Adjust values as needed for smoothness
                    confiner.InvalidatePathCache(); // Update the confiner's path cache
                    //Debug.Log("Camera confiner updated on RunCamera to new bounding shape with damping");
                }
                else
                {
                    //Debug.LogWarning("CinemachineConfiner extension not found on RunCamera");
                }
            }
            else
            {
                //Debug.LogWarning("RunCamera not found among State-Driven Camera's children");
            }
        }
        else
        {
            //Debug.LogWarning("State-Driven Camera not assigned");
        }
    }

    // Method to update camera confiner to the current floor's boundaries
    public void UpdateCameraConfiner()
    {
        //Debug.Log("Updating camera confiner for current floor: " + currentFloor.name);
        if (currentFloor != null)
        {
            Transform cameraBoundaries = currentFloor.transform.Find("CameraBoundaries");
            if (cameraBoundaries != null)
            {
                Collider2D boundaryCollider = cameraBoundaries.GetComponent<Collider2D>();
                if (boundaryCollider != null)
                {
                    SetCameraConfiner(boundaryCollider);
                }
                else
                {
                    //Debug.LogWarning("CameraBoundaries does not have a Collider2D component");
                }
            }
            else
            {
                //Debug.LogWarning("CameraBoundaries child not found in current floor");
            }
        }
    }

    // Public method to get the previous player checkpoint
    public Transform GetPreviousPlayerCheckpoint()
    {
        return previousPlayerCheckpoint;
    }
}
