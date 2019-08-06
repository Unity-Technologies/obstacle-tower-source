using System.Collections.Generic;
using UnityEngine;
using ObstacleTowerGeneration.LayoutGrammar;
using ObstacleTowerGeneration.MissionGraph;
using ObstacleTowerGeneration;

/// <summary>
/// Template-based room generator. Uses templates defined in
/// `ObstacleTower/Resources/Templates` to define RoomDefinition.
/// </summary>
public class TemplateRoomGenerator : RoomGenerator
{
    private Dictionary<string, string[]> templateDict;

    private List<RoomDefinition> normalTemplates;
    private List<RoomDefinition> puzzleTemplates;
    private List<RoomDefinition> startTemplates;
    private List<RoomDefinition> endTemplates;
    private List<RoomDefinition> keyTemplates;
    private List<RoomDefinition> basementTemplates;

    public TemplateRoomGenerator()
    {
        templateDict = new Dictionary<string, string[]>();
    }

    public List<RoomDefinition> LoadTemplates(string path)
    {
        List<RoomDefinition> templates = new List<RoomDefinition>();

        string[] text;
        if (templateDict.ContainsKey(path))
        {
            text = templateDict[path];
        }
        else
        {
            var assetString = Resources.Load<TextAsset>(path).text;
            text = Helper.SplitLines(assetString);
            templateDict[path] = text;
        }

        var roomLinesRead = 0;
        RoomDefinition room = new RoomDefinition(0, NodeType.Any);
        for (int i = 0; i < text.Length; i++)
        {
            string line = text[i];
            if (line == "")
            {
                room = MakeOuterModuleGrid(room, roomLinesRead + 2);
                room = SetDoors(room);
                templates.Add(room);
                roomLinesRead = 0;
            }
            else if (line.Length == 1)
            {
            }
            else
            {
                string[] blocks = line.Split(' ');
                if (roomLinesRead == 0)
                {
                    int roomSize = blocks.Length;
                    room = new RoomDefinition(roomSize + 2, NodeType.Any);
                }

                for (int j = 0; j < blocks.Length; j++)
                {
                    room.moduleGrid[roomLinesRead + 1, j + 1] = (int) ModuleLookUp(blocks[j][0]);
                    room.itemGrid[roomLinesRead + 1, j + 1] = (int) ItemLookUp(blocks[j][1]);
                }

                roomLinesRead += 1;
            }
        }

        return templates;
    }

    public static ModuleTypes ModuleLookUp(char character)
    {
        ModuleTypes[] moduleList;
        switch (character)
        {
            case 'p':
                // All small platforms with random probability.
                moduleList = new[]
                {
                    ModuleTypes.PlatformSmall,
                    ModuleTypes.PlatformSmallOscY,
                    ModuleTypes.PlatformSmallOscYSpinning
                };
                return moduleList[Random.Range(0, moduleList.Length)];
            case 't':
                // Tall platform with random probability.
                moduleList = new[]
                {
                    ModuleTypes.PlatformTall,
                    ModuleTypes.PlatformLarge
                };
                return moduleList[Random.Range(0, moduleList.Length)];
            case 'm':
                // Small platform with random probability.
                moduleList = new[]
                {
                    ModuleTypes.PlatformSmall,
                    ModuleTypes.PlatformLarge
                };
                return moduleList[Random.Range(0, moduleList.Length)];
            case 'w':
                // All walkable modules with random probability.
                moduleList = new[]
                {
                    ModuleTypes.PlatformSmall,
                    ModuleTypes.PlatformLargeOscY,
                    ModuleTypes.PlatformStack1,
                    ModuleTypes.PlatformStack2,
                    ModuleTypes.PlatformLarge
                };
                return moduleList[Random.Range(0, moduleList.Length)];
            case 'h':
                // Hazards
                moduleList = new[]
                {
                    ModuleTypes.PlatformHazard1Arm,
                    ModuleTypes.PlatformHazard4Arm,
                    ModuleTypes.PlatformSmallHazardSpinning
                };
                return moduleList[Random.Range(0, moduleList.Length)];
            case 'l':
                // procedural PlatformLargeOscY
                moduleList = new[]
                {
                    ModuleTypes.PlatformLarge,
                    ModuleTypes.PlatformLargeOscY,
                };
                return moduleList[Random.Range(0, moduleList.Length)];
            case 'c':
                // Oscillating platform columns
                moduleList = new[]
                {
                    ModuleTypes.PlatformLarge,
                    ModuleTypes.PlatformLargeOscY,
                    ModuleTypes.PlatformLargeColumnOscY,
                    ModuleTypes.PlatformLargeColumn2OscY
                };
                return moduleList[Random.Range(0, moduleList.Length)];
            case 's':
                // Platform stacks
                moduleList = new[]
                {
                    ModuleTypes.PlatformStack1,
                    ModuleTypes.PlatformStack2,
                    ModuleTypes.PlatformStack3,
                    ModuleTypes.PlatformLarge
                };
                return moduleList[Random.Range(0, moduleList.Length)];
            case 'i':
                //Difficulty 3
                moduleList = new[]
                {
                    ModuleTypes.Pit,
                    ModuleTypes.PlatformTall,
                    ModuleTypes.PlatformSmall,
                    ModuleTypes.PlatformLarge
                };
                return moduleList[Random.Range(0, moduleList.Length)];
            case 'j':
                //pit or tall
                moduleList = new[]
                {
                    ModuleTypes.Pit,
                    ModuleTypes.PlatformTall,
                };
                return moduleList[Random.Range(0, moduleList.Length)];
            case 'f':
                // 50% Change of either normal platform or falling platform.
                moduleList = new[] {ModuleTypes.PlatformSmall, ModuleTypes.PlatformSmallOscY};
                return moduleList[Random.Range(0, moduleList.Length)];
            case 'G':
                return ModuleTypes.PlatformLarge;
            case 'P':
                return ModuleTypes.Pit;
            case 'Q':
                return ModuleTypes.PlatformHazard4Arm;
            case 'T':
                return ModuleTypes.PlatformTall;
            case 'F':
                return ModuleTypes.PlatformLargeOscY;
            case 'M':
                return ModuleTypes.PlatformSmall;
            case 'S':
                return ModuleTypes.PlatformStack1;
            case 'H':
                return ModuleTypes.PlatformStack2;
            case 'J':
                return ModuleTypes.PlatformStack3;
            case 'Z':
                return ModuleTypes.PlatformMoving;
            case 'Y':
                return ModuleTypes.PlatformTarget;
            case 'I':
                return ModuleTypes.PlatformColumn;
            case 'V':
                return ModuleTypes.PlatformLargeColumnOscY;
            case 'K':
                return ModuleTypes.PlatformLargeColumn2OscY;
            case 'N':
                return ModuleTypes.PlatformSmallOscY;
            case 'A':
                return ModuleTypes.PlatformSmallOscYSpinning;
            default:
                return ModuleTypes.PlatformLarge;
        }
    }

    public static ItemTypes ItemLookUp(char character)
    {
        switch (character)
        {
            case 'O':
                return ItemTypes.Orb;
            case 'K':
                return ItemTypes.Key;
            case 'X':
                return ItemTypes.None;
            case 'B':
                return ItemTypes.Block;
            case 'T':
                return ItemTypes.BlockTrigger;
            case 'R':
                return ItemTypes.BlockReset;
            case 'o':
                // 50% Chance of orb.
                return Random.Range(0, 3) == 0 ? ItemTypes.Orb : ItemTypes.None;
            case 'E':
                return ItemTypes.RobotStatic;
            case 'e':
                // 30% Chance of static robot.
                return Random.Range(0, 3) == 0 ? ItemTypes.RobotStatic : ItemTypes.None;
            case 'Y':
                return ItemTypes.RobotPatrol;
            case 'C':
                return ItemTypes.RobotPatrolEndWaypoint;
            case 'U':
                return ItemTypes.RobotChasing;
            default:
                return ItemTypes.None;
        }
    }

    internal override RoomDefinition GenerateRoom(int targetDifficulty, int roomSize, Cell roomCell)
    {
        size = roomSize;
        doors = roomCell.doorTypes;
        roomType = roomCell.node.type;

        switch (roomType)
        {
            case NodeType.Key:
                keyTemplates = LoadTemplates($"Templates/{roomSize - 2}/{targetDifficulty}/keys");
                return keyTemplates[Random.Range(0, keyTemplates.Count)];
            case NodeType.End:
                endTemplates = LoadTemplates($"Templates/{roomSize - 2}/{targetDifficulty}/ends");
                return endTemplates[Random.Range(0, endTemplates.Count)];
            case NodeType.Start:
                startTemplates = LoadTemplates($"Templates/{roomSize - 2}/{targetDifficulty}/starts");
                return startTemplates[Random.Range(0, startTemplates.Count)];
            case NodeType.Puzzle:
                puzzleTemplates = LoadTemplates($"Templates/{roomSize - 2}/{targetDifficulty}/puzzles");
                return puzzleTemplates[Random.Range(0, puzzleTemplates.Count)];
            case NodeType.Normal:
                normalTemplates = LoadTemplates($"Templates/{roomSize - 2}/{targetDifficulty}/normals");
                return normalTemplates[Random.Range(0, normalTemplates.Count)];
            case NodeType.Lock:
                normalTemplates = LoadTemplates($"Templates/{roomSize - 2}/{targetDifficulty}/normals");
                return normalTemplates[Random.Range(0, normalTemplates.Count)];
            case NodeType.Basement:
                basementTemplates = LoadTemplates($"Templates/{roomSize - 2}/{targetDifficulty}/basements");
                return basementTemplates[0];
            case NodeType.Connection:
                normalTemplates = LoadTemplates($"Templates/{roomSize - 2}/{targetDifficulty}/normals");
                return normalTemplates[Random.Range(0, normalTemplates.Count)];
            default:
                normalTemplates = LoadTemplates($"Templates/{roomSize - 2}/{targetDifficulty}/normals");
                return normalTemplates[Random.Range(0, normalTemplates.Count)];
        }
    }
}
