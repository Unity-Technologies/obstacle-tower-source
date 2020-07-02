using UnityEngine;
using ObstacleTowerGeneration.LayoutGrammar;

/// <summary>
/// Responsible for instantiating game objects according to floor layouts.
/// </summary>
public class FloorBuilder : MonoBehaviour
{
    [HideInInspector] public bool fixedTower;
    [HideInInspector] public int totalFloors;
    [HideInInspector] public int towerNumber;
    [HideInInspector] public int startingFloorNumber;
    [HideInInspector] public EnvironmentParameters environmentParameters;

    [Header("Linked Objects")] 
    public GameObject roomPrefab;
    public Light sun;
    public GameObject cameraPivot;
    public GameObject agent;

    [HideInInspector] public RoomBuilder roomBuilder;
    [HideInInspector] public bool hasInitialized;
    [HideInInspector] public int floorNumber;
    private KeyController keyController;
    private FloorGenerator floorGenerator;
    private FloorGenerator.FloorLayout[] floors;    
    private int lastTowerNumber;

    public void Initialize()
    {
        floorGenerator = new FloorGenerator(environmentParameters);
        roomBuilder = roomPrefab.GetComponent<RoomBuilder>();
        hasInitialized = true;
        keyController = FindObjectOfType<KeyController>();
    }

    public void PlaceAgent(Transform targetTransform)
    {
        var targetPosition = targetTransform.position;
        agent.GetComponent<AgentAnimator>().ResetAnimator();
        agent.transform.position = targetPosition;
        agent.transform.rotation = targetTransform.rotation;
        agent.transform.Rotate(Vector3.up, +90f);
        cameraPivot.transform.position = targetPosition;
        cameraPivot.transform.rotation = targetTransform.rotation;
        cameraPivot.transform.Rotate(Vector3.up, +90f);
        agent.transform.position += 1.5f * agent.transform.forward;
        cameraPivot.transform.position += 1.5f * agent.transform.forward;
        var agentRb = agent.GetComponent<Rigidbody>();
        agentRb.velocity = Vector3.zero;
        agentRb.angularVelocity = Vector3.zero;
        agentRb.position = targetPosition + 1.5f * agent.transform.position;
        agentRb.rotation = agent.transform.rotation;
    }

    public void IncrementFloorNumber()
    {
        if (floorNumber + 1 < totalFloors)
        {
            floorNumber += 1;
        }
        else
        {
            Debug.Log("Agent reached the top of the Obstacle Tower.");
            agent.GetComponent<ObstacleTowerAgent>().EndEpisode();
        }
    }

    public void Reset()
    {
        floorNumber = startingFloorNumber;
        if (fixedTower)
        {
            if (towerNumber < 0)
            {
                towerNumber = 0;
            }

            if (floors == null || 
                towerNumber != lastTowerNumber || 
                !environmentParameters.Compare(floorGenerator.environmentParameters))
            {
                floorGenerator.SetEnvironmentParams(environmentParameters);
                floors = floorGenerator.GenerateAllFloors(towerNumber, totalFloors);
                lastTowerNumber = towerNumber;
            }
        }
        else
        {
            // Using System.Random to choose tower number so we don't end up choosing based on
            // the previously fixed seed for UnityEngine.Random.
            floorGenerator.SetEnvironmentParams(environmentParameters);
            towerNumber = new System.Random().Next(ObstacleTowerManager.MaxSeed);
            lastTowerNumber = towerNumber;
            floors = floorGenerator.GenerateAllFloors(towerNumber, totalFloors);
        }
        Debug.Log("Resetting episode with seed: " + towerNumber);
    }


    private void DestroyRooms()
    {
        foreach (Transform module in transform)
        {
            if (module.transform.name.Contains("Room_"))
            {
                Destroy(module.gameObject);
            }
        }
    }

    private void SetLighting(FloorGenerator.FloorLayout currentFloor)
    {
        if (environmentParameters.lightingType == LightingType.Fixed)
        {
            sun.shadows = LightShadows.None;
            sun.intensity = 0.0f;
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = currentFloor.sunRotation;
            sun.intensity = currentFloor.sunIntensity;
            sun.color = currentFloor.sunColor;
            RenderSettings.ambientLight = new Color(0.31f, 0.31f, 0.31f);
        }
    }

    public void ResetFloor()
    {
        if (keyController)
        {
            keyController.ResetKeys();
        }

        agent.GetComponent<ObstacleTowerAgent>().ReparentAgent();
        DestroyRooms();

        var currentFloor = floors[floorNumber];
        SetLighting(currentFloor);

        for (int i = 0; i < currentFloor.cellLayout.GetLength(0); i++)
        {
            for (int j = 0; j < currentFloor.cellLayout.GetLength(1); j++)
            {
                if (currentFloor.cellLayout[i, j] != null)
                {
                    Cell roomCell = currentFloor.cellLayout[i, j];
                    var room = Instantiate(roomPrefab, transform);
                    int roomLength = (currentFloor.floorRoomSize - 1) * 5;
                    room.transform.position = new Vector3(i * roomLength, 0, j * roomLength);
                    if (roomCell != null)
                    {
                        room.name = "Room_" + roomCell.node.type + "_" + i + "_" + j;
                    }
                    else
                    {
                        room.name = "Room_" + "Null" + "_" + i + "_" + j;
                    }

                    room.GetComponent<RoomBuilder>().ResetRoom(currentFloor.floorLayout[i, j]);
                }
            }
        }
    }
}
