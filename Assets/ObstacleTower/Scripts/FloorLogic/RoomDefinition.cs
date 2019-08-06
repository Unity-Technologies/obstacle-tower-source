using ObstacleTowerGeneration.MissionGraph;
using UnityEngine;

/// <summary>
/// Defines contents and layout of a given room.
/// </summary>
public struct RoomDefinition
{
    public int[,] moduleGrid;
    public int[,] itemGrid;
    public int size;
    public VisualTheme visualTheme;
    public NodeType roomType;

    public RoomDefinition(int size, NodeType type)
    {
        this.size = size;
        moduleGrid = new int[size, size];
        itemGrid = new int[size, size];
        visualTheme = VisualTheme.Ancient;
        roomType = type;
    }

    public void SetModulePosition(int x, int z, ModuleTypes moduleType)
    {
        moduleGrid[x, z] = moduleType.GetHashCode();
    }

    public ModuleTypes GetModuleType(int x, int z)
    {
        return (ModuleTypes) moduleGrid[x, z];
    }

    public void SetItemPosition(int x, int z, ItemTypes itemType)
    {
        itemGrid[x, z] = itemType.GetHashCode();
    }

    public ItemTypes GetItemType(int x, int z)
    {
        return (ItemTypes) moduleGrid[x, z];
    }
    
    public void Print()
    {
        string printModuleList = "Module Layout:\n";
        string printItemList = "Item Layout:\n";
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                printModuleList += moduleGrid[i, j];
                printItemList += itemGrid[i, j];
            }

            printModuleList += '\n';
            printItemList += '\n';
        }

        Debug.Log(printModuleList);
        Debug.Log(printItemList);
    }
    
}

public enum ModuleTypes
{
    Pit,
    Wall,
    WallCorner,
    PlatformLarge,
    PlatformColumn,
    PlatformStack2,
    PlatformLargeColumnOscY,
    PlatformStack1,
    PlatformLargeOscY,
    PlatformSmall,
    DoorEntrance,
    DoorExit,
    DoorBetween,
    DoorPuzzle,
    DoorLocked,
    PlatformTall,
    PlatformMoving,
    PlatformSmallOscY,
    WallWindow,
    PlatformStack3,
    PlatformHazard4Arm,
    PlatformHazard1Arm,
    PlatformSmallHazardSpinning,
    PlatformSmallOscYSpinning,
    PlatformLargeColumn2OscY,
    DoorLever,
    DoorOneWay,
    PlatformTarget
}

public enum ItemTypes
{
    None,
    Key,
    Orb,
    Block,
    BlockTrigger,
    RobotStatic,
    RobotPatrol,
    RobotPatrolEndWaypoint,
    RobotChasing,
    BlockReset
}

public enum VisualTheme
{
    Ancient,
    Moorish,
    Industrial,
    Modern,
    Future
}
