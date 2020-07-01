using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using ObstacleTowerGeneration;
using ObstacleTowerGeneration.LayoutGrammar;
using ObstacleTowerGeneration.MissionGraph;
using System.Linq;

/// <summary>
/// Responsible for generating floor layouts based on environment parameters.
/// </summary>
public class FloorGenerator
{
    public EnvironmentParameters environmentParameters;
    private readonly TemplateRoomGenerator templateGenerator;
    private readonly bool debug;
    
    public FloorGenerator(
        EnvironmentParameters environmentParameters, 
        bool debug = false)
    {
        this.environmentParameters = environmentParameters;
        this.debug = debug;
        
        templateGenerator = new TemplateRoomGenerator();
    }
    
    public struct FloorLayout
    {
        public RoomDefinition[,] floorLayout;
        internal Cell[,] cellLayout;
        public int floorRoomSize;
        public Color sunColor;
        public float sunIntensity;
        public Quaternion sunRotation;
    }

    public void SetEnvironmentParams(EnvironmentParameters envParams)
    {
        environmentParameters = envParams;
    }
    
    public  FloorLayout[] GenerateAllFloors(int towerNumber, int numberFloors)
    {
        Random.InitState(towerNumber);

        System.Random dungeonRandom = new System.Random(towerNumber);
        var floors = new FloorLayout[numberFloors];

        try
        {
            for (var i = 0; i < numberFloors; i++)
            {
                if (i == 0)
                {
                    floors[i] = GenerateBasement();
                }
                else
                {
                    floors[i] = GenerateFloor(i, dungeonRandom);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
#if UNITY_EDITOR
            Debug.LogError("There was an error generating the floor definition. Leaving play mode.");
            UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
        }

        return floors;
    }

    private  FloorLayout GenerateFloor(int floorNum, System.Random randomGen)
    {
        var floorSize = SetFloorRoomSize(floorNum);
        var floorRecipeLength = SetRecipeLength(floorNum);
        var floorTheme = SetVisualTheme(floorNum);
        var roomDifficulty = SetRoomDifficulty(floorNum);
        var recipeName = GetRecipe();

        var layout = new FloorLayout {floorRoomSize = floorSize};

        DungeonResult results = Program.GenerateDungeon(recipeLength: floorRecipeLength, randomGen: randomGen,
            recipeName: recipeName);

        if (results.layoutMap == null)
        {
            Debug.Log("Generated layout was invalid.");
            Application.Quit();
        }
        else
        {
            Cell[,] cellGrid = results.layoutMap.Get2DMap();
            if (debug)
            {
                Debug.Log(results.layoutMap.ToString());
            }

            layout.floorLayout = new RoomDefinition[cellGrid.GetLength(0), cellGrid.GetLength(1)];

            for (int i = 0; i < cellGrid.GetLength(0); i++)
            {
                for (int j = 0; j < cellGrid.GetLength(1); j++)
                {
                    if (cellGrid[i, j] != null)
                    {
                        if (cellGrid[i, j].type == CellType.Connection)
                        {
                            cellGrid[i, j].node = new Node(0, 0, NodeType.Connection);
                        }
                        layout.floorLayout[i, j] =
                            templateGenerator.GenerateRoom(roomDifficulty, floorSize, cellGrid[i, j]);
                        layout.floorLayout[i, j].visualTheme = floorTheme;
                        layout.floorLayout[i, j].roomType = cellGrid[i, j].node.type;
                    }
                    
                }
            }

            layout.cellLayout = cellGrid;
        }

        SetSunProperties(ref layout);

        return layout;
    }


    private void SetSunProperties(ref FloorLayout layout)
    {
        var rangeSet = new[]
            {Random.Range(23f, 77f), Random.Range(109f, 164f), Random.Range(199f, 243f), Random.Range(291f, 337f)};
        if (environmentParameters.lightingType == LightingType.Extreme)
        {
            layout.sunRotation = Quaternion.Euler(
                Random.Range(0, 90), rangeSet[Random.Range(0, rangeSet.Length)], 0);
            layout.sunIntensity = Random.Range(0f, 10f);
            layout.sunColor = new Color(
                Random.Range(0.0f, 1f), Random.Range(0.0f, 1f), Random.Range(0.0f, 1f));
        }
        else
        {
            layout.sunRotation = Quaternion.Euler(
                Random.Range(23, 77), rangeSet[Random.Range(0, rangeSet.Length)], 0);
            layout.sunIntensity = Random.Range(3f, 7f);
            layout.sunColor = new Color(
                Random.Range(0.65f, 1f), Random.Range(0.8f, 1f), Random.Range(0.8f, 1f));
        }
    }


    private  FloorLayout GenerateBasement()
    {
        Cell[,] cellGrid = new Cell[1, 1];

        Node node = new Node(0, 0, NodeType.Basement);

        List<int> possibleDoors = new List<int> {0, 1, 2, 3};
        int doorStart = possibleDoors[Random.Range(0, possibleDoors.Count)];
        possibleDoors.Remove(doorStart);
        int doorEnd = possibleDoors[Random.Range(0, possibleDoors.Count)];

        cellGrid[0, 0] = new Cell(0, 0, CellType.Normal, node);
        cellGrid[0, 0].doorTypes[doorStart] = DoorType.Start;
        cellGrid[0, 0].doorTypes[doorEnd] = DoorType.Exit;

        var layout = new FloorLayout
        {
            floorRoomSize = 5,
            floorLayout = new RoomDefinition[1, 1],
            cellLayout = cellGrid
        };

        SetSunProperties(ref layout);
        layout.floorLayout[0, 0] = templateGenerator.GenerateRoom(0, 5, cellGrid[0, 0]);
        layout.floorLayout[0, 0].visualTheme = SetVisualTheme(0);

        return layout;
    }
    
    private  string GetRecipe()
    {
        switch (environmentParameters.allowedRoomTypes)
        {
            case AllowedRoomTypes.Normal:
                return "graphRecipeNormal";
            case AllowedRoomTypes.PlusKey:
                return "graphRecipeKey";
            case AllowedRoomTypes.PlusPuzzle:
                switch (environmentParameters.allowedFloorLayouts)
                {
                    case AllowedFloorLayouts.Linear:
                        return "graphRecipeSimple";
                    case AllowedFloorLayouts.PlusBranching:
                        return "graphRecipeBranching";
                    case AllowedFloorLayouts.PlusCircling:
                        return "graphRecipe";
                }

                return "graphRecipe";
            default:
                return "graphRecipe";
        }
    }


    /// <summary>
    /// Sets the visual theme for the floor.
    /// Change these threshold values to load floor themes at different points in the tower.
    /// Valid value: VisualTheme.Ancient, VisualTheme.Moorish, VisualTheme.Industrial,
    ///             VisualTheme.Modern, VisualTheme.Future
    /// </summary>
    public  VisualTheme SetVisualTheme(int floorNum)
    {
        switch (environmentParameters.themeParameter)
        {
            case VisualThemeParameter.One:
                return environmentParameters.defaultTheme;
            case VisualThemeParameter.Serial:
                if (floorNum < 10)
                {
                    return VisualTheme.Ancient;
                }

                if (floorNum < 20)
                {
                    VisualTheme[] themes = {VisualTheme.Ancient, VisualTheme.Moorish};
                    return themes[Random.Range(0, 2)];
                }

                if (floorNum < 30)
                {
                    return VisualTheme.Moorish;
                }

                if (floorNum < 40)
                {
                    VisualTheme[] themes = {VisualTheme.Moorish, VisualTheme.Industrial};
                    return themes[Random.Range(0, 2)];
                }

                if (floorNum < 50)
                {
                    return VisualTheme.Industrial;
                }

                if (floorNum < 60)
                {
                    VisualTheme[] themes = {VisualTheme.Industrial, VisualTheme.Modern};
                    return themes[Random.Range(0, 2)];
                }

                if (floorNum < 70)
                {
                    return VisualTheme.Modern;
                }

                if (floorNum < 80)
                {
                    VisualTheme[] themes = {VisualTheme.Modern, VisualTheme.Future};
                    return themes[Random.Range(0, 2)];
                }

                return VisualTheme.Future;
            case VisualThemeParameter.Random:
                List<VisualTheme> themes_random = new List<VisualTheme>();
                if (environmentParameters.use_ancient)
                    themes_random.Add(VisualTheme.Ancient);
                if (environmentParameters.use_moorish)
                    themes_random.Add(VisualTheme.Moorish);
                if (environmentParameters.use_industrial)
                    themes_random.Add(VisualTheme.Industrial);
                if (environmentParameters.use_modern)
                    themes_random.Add(VisualTheme.Modern);
                if (environmentParameters.use_future)
                    themes_random.Add(VisualTheme.Future);
                if (themes_random.Count < 1)
                {
                    Debug.LogError("At least one visual theme has to be enabled to make use of VisualThemeParameter.Random.");
                }
                return themes_random[Random.Range(0, themes_random.Count)];
            default:
            {
                return VisualTheme.Ancient;
            }
        }
    }

    /// <summary>
    /// Sets the length of the recipe used to generate the floor layout.
    /// Recipes are located in `ObstacleTower/Resources/FloorGeneration`.
    /// Valid value range: 1 ~ 17
    /// </summary>
    public  int SetRecipeLength(int floorNum)
    {
        int recipeLength;
        if (floorNum < 2)
        {
            recipeLength = 1;
        }
        else if (floorNum < 5)
        {
            recipeLength = 2;
        }
        else if (floorNum < 10)
        {
            recipeLength = 3;
        }
        else if (floorNum < 20)
        {
            recipeLength = 4;
        }
        else if (floorNum < 30)
        {
            recipeLength = 5;
        }
        else if (floorNum < 40)
        {
            recipeLength = 7;
        }
        else if (floorNum < 50)
        {
            recipeLength = 9;
        }
        else if (floorNum < 60)
        {
            recipeLength = 11;
        }
        else if (floorNum < 80)
        {
            recipeLength = 13;
        }
        else if (floorNum < 90)
        {
            recipeLength = 15;
        }
        else
        {
            recipeLength = 17;
        }

        return recipeLength;
    }

    /// <summary>
    /// Sets the size of the rooms based on current floor.
    /// Note that navigable size is two less than the return value here due to wall modules.
    /// Valid value range: > 3
    /// </summary>
    public  int SetFloorRoomSize(int floorNum)
    {
        switch (environmentParameters.allowedRoomModules)
        {
            case AllowedRoomModules.None:
                return 5;
            case AllowedRoomModules.Easy:
                if (floorNum < 25)
                {
                    return 5;
                }

                if (floorNum < 50)
                {
                    int[] size = {5, 6};
                    return size[Random.Range(0, 2)];
                }

                return 6;
            case AllowedRoomModules.All:
                if (floorNum < 15)
                {
                    return 5;
                }

                if (floorNum < 30)
                {
                    int[] size = {5, 6};
                    return size[Random.Range(0, 2)];
                }

                if (floorNum < 45)
                {
                    return 6;
                }

                if (floorNum < 60)
                {
                    int[] size = {6, 7};
                    return size[Random.Range(0, 2)];
                }

                return 7;
            default:
                return 5;
        }
    }

    /// <summary>
    /// Determines which templates to load when populating the rooms on the floor.
    /// Valid value range: 0 ~ 7
    /// </summary>
    public int SetRoomDifficulty(int floorNum)
    {
        switch (environmentParameters.allowedRoomModules)
        {
            case AllowedRoomModules.None:
                if (floorNum < 2)
                {
                    return 0;
                }

                return 1;
            case AllowedRoomModules.Easy:
                if (floorNum < 2)
                {
                    return 0;
                }

                if (floorNum < 25)
                {
                    return 1;
                }

                if (floorNum < 50)
                {
                    return 2;
                }

                return 3;
            case AllowedRoomModules.All:
                if (floorNum < 2)
                {
                    return 0;
                }

                if (floorNum < 7)
                {
                    return 1;
                }

                if (floorNum < 15)
                {
                    return 2;
                }

                if (floorNum < 22)
                {
                    return 3;
                }

                if (floorNum < 30)
                {
                    return 4;
                }

                if (floorNum < 60)
                {
                    return 5;
                }

                if (floorNum < 90)
                {
                    return 6;
                }

                return 7;
            default:
                return 0;
        }
    }
}