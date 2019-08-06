using System.Collections.Generic;
using ObstacleTowerGeneration.LayoutGrammar;
using ObstacleTowerGeneration.MissionGraph;

/// <summary>
/// Base class responsible for generating room layout.
/// </summary>
public abstract class RoomGenerator
{
    internal int size;
    internal int innerSize;
    internal int difficulty;
    
    public int[,] innerItemGrid;
    public int[,] innerModuleGrid;
    internal RoomDefinition workingRoom;
    internal ModuleTypes defaultModule;
    internal NodeType roomType;
    internal DoorType[] doors;

    public static RoomDefinition MakeOuterModuleGrid(RoomDefinition room, int totalSize)
    {
        for (var i = 0; i < totalSize; i++)
        {
            for (var j = 0; j < totalSize; j++)
            {
                // Create corner.
                if (i == 0 && j == 0 || i == 0 && j == totalSize - 1 || 
                    i == totalSize - 1 && j == 0 || i == totalSize - 1 && j == totalSize - 1)
                {
                    room.SetModulePosition(i, j, ModuleTypes.WallCorner);
                }

                // Check for edge and create wall.
                else if ((i == 0 || i == totalSize - 1 || j == 0 || j == totalSize - 1) && 
                    (int)room.GetModuleType(i, j) == 0)
                {
                    room.SetModulePosition(i, j, ModuleTypes.WallWindow);
                }

            }
        }

        return room;
    }

    internal int[,] InitializeGridWithValue(int[,] grid, int value)
    {
        for (int i = 0; i < innerSize; i++)
        {
            for (int j = 0; j < innerSize; j++)
            {
                grid[i, j] = value;
            }
        }
        return grid;
    }

    public static void ApplyInnerGrid(int[,] innerGrid, int[,] wholeGrid)
    {
        var innerSize = innerGrid.GetLength(0);
        for (var i = 0; i < innerSize; i++)
        {
            for (var j = 0; j < innerSize; j++)
            {
                if (wholeGrid[i + 1, j + 1] == 0)
                    wholeGrid[i + 1, j + 1] = innerGrid[i, j];
            }
        }
    }

    public static int PositionToNumber(int x, int y, int givenSize)
    {
        return x * givenSize + y;
    }

    public static int[] NumberToPosition(int position, int givenSize)
    {
        int x = position / givenSize;
        int y = position % givenSize;
        return new[] {x, y};
    }

    internal void RemoveFromList(List<int> objectSpaces, int position)
    {
        if (objectSpaces.Contains(position))
        {
            objectSpaces.Remove(position);
        }
    }

    internal RoomDefinition SetDoors(RoomDefinition room)
    {
        bool placedSpecialDoor = false;
        for (int i = 0; i < doors.Length; i++)
        {
            var doorType = doors[i];

            ModuleTypes doorModule;
            switch (doorType)
            {
                case DoorType.Open:
                    doorModule = ModuleTypes.DoorBetween;
                    break;
                case DoorType.KeyLock:
                    doorModule = ModuleTypes.DoorLocked;
                    break;
                case DoorType.LeverLock:
                    doorModule = ModuleTypes.DoorLever;
                    break;
                case DoorType.OneWay:
                    doorModule = ModuleTypes.DoorOneWay;
                    break;
                case DoorType.PuzzleLock:
                    doorModule = ModuleTypes.DoorPuzzle; 
                    break;
                default:
                    doorModule = ModuleTypes.Wall;
                    if (!placedSpecialDoor)
                    {
                        if (roomType == NodeType.Start && doorType == 0)
                        {
                            doorModule = ModuleTypes.DoorEntrance;
                            placedSpecialDoor = true;
                        }
                        if (roomType == NodeType.End && doorType == 0)
                        {
                            doorModule = ModuleTypes.DoorExit;
                            placedSpecialDoor = true;
                        }

                        if (roomType == NodeType.Basement && doorType == DoorType.Start)
                        {
                            doorModule = ModuleTypes.DoorEntrance;
                        }

                        if (roomType == NodeType.Basement && doorType == DoorType.Exit)
                        {
                            doorModule = ModuleTypes.DoorExit;
                        }
                    }
                    break;
            }

            if (doorModule != ModuleTypes.Wall)
            {
                room = PlaceDoor(room, i, doorModule, true);
            }
        }

        return room;
    }

    internal virtual RoomDefinition PlaceDoor(RoomDefinition room, int wallPosition, ModuleTypes doorModule, bool addGround)
    {
        var doorPosition = 2;
        if (room.size == 7)
        {
            doorPosition = 3;
        }
        switch (wallPosition)
        {
            case 1:
                if (addGround)
                {
                    room.SetModulePosition(1, doorPosition, ModuleTypes.PlatformLarge);
                }
                for (int i = 1; i < size - 1; i++)
                {
                    room.SetModulePosition(0, i, ModuleTypes.Wall);
                }
                room.SetModulePosition(0, doorPosition, doorModule);
                break;
            case 3:
                if (addGround)
                {
                    room.SetModulePosition(doorPosition, 1, ModuleTypes.PlatformLarge);
                }
                for (int i = 1; i < size - 1; i++)
                {
                    room.SetModulePosition(i, 0, ModuleTypes.Wall);
                }
                room.SetModulePosition(doorPosition, 0, doorModule);
                break;
            case 2:
                if (addGround)
                {
                    room.SetModulePosition(doorPosition, size - 2, ModuleTypes.PlatformLarge);
                }
                for (int i = 1; i < size - 1; i++)
                {
                    room.SetModulePosition(i, size - 1, ModuleTypes.Wall);
                }
                room.SetModulePosition(doorPosition, size - 1, doorModule);
                break;
            case 0:
                if (addGround)
                {
                    room.SetModulePosition(size - 2, doorPosition, ModuleTypes.PlatformLarge);
                }
                for (int i = 1; i < size - 1; i++)
                {
                    room.SetModulePosition(size - 1, i, ModuleTypes.Wall);
                }
                room.SetModulePosition(size - 1, doorPosition, doorModule);
                break;
        }

        return room;
    }

    internal ModuleTypes SetDefaultModule()
    {
        ModuleTypes newDefault = ModuleTypes.PlatformLarge;
        if (roomType == NodeType.Start || roomType == NodeType.End)
        {
            newDefault = ModuleTypes.PlatformLarge;
        }
        if (roomType == NodeType.Normal)
        {
            if (difficulty < 2)
            {
                newDefault = ModuleTypes.PlatformLargeOscY;
            }
            else if (difficulty >= 2 && difficulty < 5)
            {
                newDefault = ModuleTypes.PlatformLargeOscY;
            }
            else
            {
                newDefault = ModuleTypes.PlatformLargeOscY;
            }
        }
        return newDefault;
    }

    internal virtual RoomDefinition GenerateRoom(int targetDifficulty, int roomSize, Cell roomCell)
    {        
        return new RoomDefinition();
    }
}
