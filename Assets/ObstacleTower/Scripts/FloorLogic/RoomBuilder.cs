using System.Collections.Generic;
using ObstacleTowerGeneration.MissionGraph;
using UnityEngine;

/// <summary>
/// Responsible for instantiating modules and items within each room of a floor.
/// </summary>
public class RoomBuilder : MonoBehaviour
{
    [Header("Item Prefabs")] 
    public GameObject orbPrefab;
    public GameObject blockPrefab;
    public GameObject blockTriggerPrefab;
    public GameObject keyPrefab;
    public GameObject robotStaticPrefab;
    public GameObject robotPatrolPrefab;
    public GameObject robotPatrolEndWaypointPrefab;
    public GameObject robotChasingPrefab;
    public GameObject blockResetPrefab;

    private const float ModuleSize = 5f;
    private Dictionary<ItemTypes, GameObject> itemDictionary;
    private RoomDefinition room;
    private VisualTheme roomTheme;

    void Awake()
    {
        itemDictionary = new Dictionary<ItemTypes, GameObject>
        {
            {ItemTypes.Key, keyPrefab},
            {ItemTypes.Orb, orbPrefab},
            {ItemTypes.Block, blockPrefab},
            {ItemTypes.BlockTrigger, blockTriggerPrefab},
            {ItemTypes.RobotStatic, robotStaticPrefab},
            {ItemTypes.RobotPatrol, robotPatrolPrefab},
            {ItemTypes.RobotChasing, robotChasingPrefab},
            {ItemTypes.RobotPatrolEndWaypoint, robotPatrolEndWaypointPrefab},
            {ItemTypes.BlockReset, blockResetPrefab}
        };
    }

    private void DestroyModules()
    {
        foreach (Transform module in transform)
        {
            if (module.transform.name.Contains("Module_") ||
                module.transform.name.Contains("Item_") ||
                module.transform.name.Contains("Scenery_"))
            {
                Destroy(module.gameObject);
            }
        }
    }

    private GameObject InstantiateModule(ModuleTypes moduleEnum)
    {
        var objectName = moduleEnum.ToString();
        var dir = "Prefabs/" + roomTheme + "/Modules/" + objectName;
        var gameObj = Resources.Load(dir) as GameObject;
        if (!gameObj)
        {
            Debug.Log($"InstantiateModule: could not instantiate {dir}");
        }
        else
        {
            gameObj = (GameObject) Instantiate(Resources.Load(dir), transform);
        }

        return gameObj;
    }

    private void InitializeModule(int x, int z)
    {
        var module = InstantiateModule(((ModuleTypes) room.moduleGrid[x, z]));
        module.name = "Module_" + (ModuleTypes) room.moduleGrid[x, z] + "_" + x + "_" + z;
        module.transform.position = new Vector3(x * ModuleSize, 0f, z * ModuleSize) + transform.position;
        AdjustObjectRotation(x, z, module);

        if ((ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.DoorEntrance)
        {
            transform.parent.GetComponent<FloorBuilder>().PlaceAgent(module.transform);
        }
    }

    private void InitializeScenery(int x, int z)
    {
        string sceneryName;
        if (!CanUseRoomColumns(room)) //check if we can spawn columns in this room
        {
            sceneryName = "SceneryC"; //SceneryC does not spawn columns
        }
        else //we can use room columns
        {
            if (x % 2 == 0 && z % 2 == 0)
            {
                sceneryName = "SceneryA";
            }
            else
            {
                sceneryName = "SceneryB";
            }
        }

        var module = (GameObject) Instantiate(Resources.Load("Prefabs/" + roomTheme + "/Scenery/" + sceneryName),
            transform);
        module.name = "Scenery_" + x + "_" + z;
        module.transform.position = new Vector3(x * ModuleSize, 0f, z * ModuleSize) + transform.position;
    }

    private void InitializeItem(int x, int z)
    {
        if (room.itemGrid[x, z] != 0)
        {
            var itemType = (ItemTypes) room.itemGrid[x, z];
            var item = Instantiate(itemDictionary[itemType], transform);
            item.name = "Item_" + itemType + "_" + x + "_" + z;
            item.transform.position = new Vector3(x * ModuleSize, 0.5f, z * ModuleSize) + transform.position;
            AdjustObjectRotation(x, z, item);

            if (itemType == ItemTypes.Key || itemType == ItemTypes.Orb || itemType == ItemTypes.Block)
            {
                if ((ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformTall)
                {
                    item.transform.position += new Vector3(0f, 5, 0f);
                }

                if ((ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformLargeColumnOscY ||
                    (ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformStack3 ||
                    (ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformLargeColumn2OscY ||
                    (ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformSmallOscYSpinning)
                {
                    item.transform.position += new Vector3(0f, 4.5f, 0f);
                }

                if ((ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformStack2 ||
                    (ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformHazard1Arm ||
                    (ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformHazard4Arm ||
                    (ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformSmallOscY)
                {
                    item.transform.position += new Vector3(0f, 2f, 0f);
                }

                if ((ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformSmallHazardSpinning)
                {
                    item.transform.position += new Vector3(0f, 3f, 0f);
                }

                if ((ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformStack1 ||
                    (ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformMoving)
                {
                    item.transform.position += new Vector3(0f, 1.0f, 0f);
                }

                if ((ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.Pit)
                {
                    item.transform.position += new Vector3(0f, 0.5f, 0f);
                }
            }

            if (itemType == ItemTypes.RobotStatic || itemType == ItemTypes.RobotPatrol ||
                itemType == ItemTypes.RobotChasing || itemType == ItemTypes.RobotPatrolEndWaypoint)
            {
                if ((ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformLarge ||
                    (ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformSmall)
                {
                    item.transform.position += new Vector3(0f, -0.5f, 0f);
                }

                if ((ModuleTypes) room.moduleGrid[x, z] == ModuleTypes.PlatformTall)
                {
                    item.transform.position += new Vector3(0f, 2.5f, 0f);
                }

                //If it's a robot set the new starting pos
                Robot robotScript = item.gameObject.GetComponent<Robot>();
                if (robotScript)
                {
                    robotScript.startingPos = item.transform.position;
                }
            }
        }
    }

    private void AdjustObjectRotation(int x, int z, GameObject module)
    {
        if (z == 0 && x > 0)
        {
            module.transform.Rotate(0f, -90f, 0f);
        }

        if (x == room.size - 1)
        {
            module.transform.Rotate(0f, 180f, 0f);
        }

        if (x == room.size - 1 && z == 0)
        {
            module.transform.Rotate(0f, -180, 0f);
        }

        if (x == room.size - 1 && z == room.size - 1)
        {
            module.transform.Rotate(0f, -90, 0f);
        }

        if (z == room.size - 1)
        {
            module.transform.Rotate(0f, 90f, 0f);
        }
    }


    //Helper method to determine whether we should use columns in this room.
    //Any column spawn checks can be put in this method. 
    bool CanUseRoomColumns(RoomDefinition floorRoom)
    {
        //Room Type Checks
        if (floorRoom.roomType == NodeType.Puzzle)
        {
            return false;
        }

        //Item Checks
        for (var i = 0; i < floorRoom.size; i++)
        {
            for (var j = 0; j < floorRoom.size; j++)
            {
                var itemType = (ItemTypes) floorRoom.itemGrid[i, j];

                //Check for chasing robots since they will go right through columns;
                if (itemType == ItemTypes.RobotChasing)
                {
                    return false;
                }
            }
        }

        return true; //all checks passed. we can use columns
    }

    public void ResetRoom(RoomDefinition floorRoom)
    {
        room = floorRoom;
        roomTheme = floorRoom.visualTheme;

        DestroyModules();

        for (var i = 0; i < room.size; i++)
        {
            for (var j = 0; j < room.size; j++)
            {
                if (i > 0 && j > 0 && i < room.size - 1 && j < room.size - 1)
                {
                    InitializeScenery(i, j);
                }

                InitializeModule(i, j);
                InitializeItem(i, j);
            }
        }
    }
}
