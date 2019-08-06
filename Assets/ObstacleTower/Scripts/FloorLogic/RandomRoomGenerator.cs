using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using ObstacleTowerGeneration.LayoutGrammar;
using ObstacleTowerGeneration.MissionGraph;

/// <summary>
/// An example of an alternative to the template-based generator described in
/// `TemplateRoomGenerator.cs`. This is not used in the game.
/// </summary>
public class RandomRoomGenerator : RoomGenerator {
    // Lists of positions where modules & items can be placed.
    private List<int> moduleSpaces = new List<int>(36);
    private List<int> itemSpaces = new List<int>(36);
    
    private readonly List<ModuleTypes> supportItems = new List<ModuleTypes>();

    public RandomRoomGenerator()
    {
        supportItems.Add(ModuleTypes.PlatformLarge);
        supportItems.Add(ModuleTypes.PlatformStack2);
        supportItems.Add(ModuleTypes.PlatformStack1);
        supportItems.Add(ModuleTypes.PlatformTall);
        supportItems.Add(ModuleTypes.PlatformLargeOscY);
    }
    
    private void MakeInnerItemGridRandom()
    {
        innerItemGrid = new int[innerSize, innerSize];
        if (roomType == NodeType.Key)
        {
            PlaceItemRandom(ItemTypes.Key, 1);
        }
        if (roomType == NodeType.Puzzle)
        {
            PlaceItemRandom(ItemTypes.Block, 1);
        }
        if (innerSize > 2 && roomType == NodeType.Normal)
        {
            PlaceItemRandom(ItemTypes.Orb, Random.Range(0, innerSize - 1));
        }
    }
    
    private void PlaceItemRandom(ItemTypes itemType, int numObjects)
    {
        var itemIndex = itemType.GetHashCode();
        for (var i = 0; i < numObjects; i++)
        {
            var listPosition = Random.Range(0, itemSpaces.Count);
            var spatialPosition = itemSpaces[listPosition];
            var absolutePosition = NumberToPosition(spatialPosition, innerSize);
            innerItemGrid[absolutePosition[0], absolutePosition[1]] = itemIndex;
            itemSpaces.RemoveAll(value => value == spatialPosition);
        }
    }

    private void MakeInnerModuleGridRandom()
    {
        innerModuleGrid = new int[innerSize, innerSize];
        innerModuleGrid = InitializeGridWithValue(innerModuleGrid, (int) defaultModule);

        var possibleModules = innerSize * innerSize - 4;
        var modulesToPlace = (int)Random.Range(possibleModules * 0.5f, possibleModules);

        for (int i = 0; i < modulesToPlace; i++)
        {
            ModuleTypes placedModule = ModuleTypes.PlatformLarge;
            if (roomType == NodeType.Start)
            {
                placedModule = ModuleTypes.PlatformLarge;
            }
            else if (roomType == NodeType.Normal ||  
                     roomType == NodeType.Key || 
                     roomType == NodeType.Lock || roomType == NodeType.End)
            {
                placedModule = (ModuleTypes) Random.Range(3, 9);
            }
            PlaceModuleRandom(placedModule, 1);
        }
    }

    private void PlaceModuleRandom(ModuleTypes moduleType, int numObjects)
    {
        var moduleIndex = moduleType.GetHashCode();
        for (var i = 0; i < numObjects; i++)
        {
            var listPosition = Random.Range(0, moduleSpaces.Count);
            var spatialPosition = moduleSpaces[listPosition];
            var absolutePosition = NumberToPosition(spatialPosition, innerSize);
            innerModuleGrid[absolutePosition[0], absolutePosition[1]] = moduleIndex;
            moduleSpaces.RemoveAt(listPosition);

            if (supportItems.Contains(moduleType))
            {
                // Prefer to place items here.
                itemSpaces.Add(spatialPosition);
                itemSpaces.Add(spatialPosition);
            }
            else
            {
                // Can't place an item here.
                itemSpaces.RemoveAll(value => value == spatialPosition);
            }
        }
    }
    
    private void SetModuleGridRandom()
    {
        workingRoom = MakeOuterModuleGrid(workingRoom, size);
        MakeInnerModuleGridRandom();
        ApplyInnerGrid(innerModuleGrid, workingRoom.moduleGrid);
    }

    private void SetItemGridRandom()
    {
        MakeInnerItemGridRandom();
        ApplyInnerGrid(innerItemGrid, workingRoom.itemGrid);
    }
    
    internal override RoomDefinition PlaceDoor(RoomDefinition room, int wallPosition, ModuleTypes doorModule, bool addGround)
    {
        switch (wallPosition)
        {
            case 1:
                room.SetModulePosition(1, 2, ModuleTypes.PlatformLarge);
                room.SetModulePosition(0, 2, doorModule);
                RemoveFromList(moduleSpaces, PositionToNumber(0, 2 - 1, innerSize));
                break;
            case 3:
                room.SetModulePosition(2, 1, ModuleTypes.PlatformLarge);
                room.SetModulePosition(2, 0, doorModule);
                RemoveFromList(moduleSpaces, PositionToNumber(2 - 1, 0, innerSize));
                break;
            case 2:
                room.SetModulePosition(2, size - 2, ModuleTypes.PlatformLarge);
                room.SetModulePosition(2, size - 1, doorModule);
                RemoveFromList(moduleSpaces, PositionToNumber(2 - 1, size - 3, innerSize));
                break;
            case 0:
                room.SetModulePosition(size - 2, 2, ModuleTypes.PlatformLarge);
                room.SetModulePosition(size - 1, 2, doorModule);
                RemoveFromList(moduleSpaces, PositionToNumber(size - 3, 2 - 1, innerSize));
                break;
        }

        return room;
    }

    internal override RoomDefinition GenerateRoom(int targetDifficulty, int roomSize, Cell roomCell)
    {
        roomType = roomCell.node.type;
        doors = roomCell.doorTypes;        
        difficulty = targetDifficulty;
        defaultModule = SetDefaultModule();
        size = roomSize;
        innerSize = size - 2;
        workingRoom = new RoomDefinition(size, roomCell.node.type);
        moduleSpaces = Enumerable.Range(0, innerSize * innerSize).ToList();
        if (roomType == NodeType.Key)
        {
            itemSpaces = Enumerable.Range(0, innerSize * innerSize).ToList();
        }
        else
        {
            itemSpaces.Clear();
        }
        workingRoom = SetDoors(workingRoom);
        SetModuleGridRandom();
        SetItemGridRandom();
        return workingRoom;
    }
}
