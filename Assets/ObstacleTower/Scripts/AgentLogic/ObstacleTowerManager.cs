using System;
using UnityEngine;
using Unity.MLAgents;
using Debug = UnityEngine.Debug;
using UnityEngine.Rendering;


/// <summary>
/// Academy for Obstacle Tower.
/// Responsible for reading relevant reset parameters, and evaluation logic.
/// </summary>
public class ObstacleTowerManager : MonoBehaviour
{
    public FloorBuilder floor;

    public const int MaxSeed = 99999;
    public const int MaxFloors = 100;

    [HideInInspector]
    public bool InferenceOn = false;

    private ObstacleTowerAgent agentComponent;
    
    public void Awake()
    {
        floor.environmentParameters = new EnvironmentParameters();
        
        agentComponent = FindObjectOfType<ObstacleTowerAgent>();
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        Academy.Instance.OnEnvironmentReset += ResetTower;
    }
    
    private void EnableInference()
    {
        InferenceOn = true;
        agentComponent.SetInference();
        Time.captureFramerate = 0;
    }

    private void EnableTraining()
    {
        InferenceOn = false;
        agentComponent.SetTraining();
        Time.captureFramerate = 60;
    }

    private void SetDefaultEnvironmentParameters()
    {
        floor.environmentParameters.lightingType = LightingType.Dynamic;
        floor.environmentParameters.themeParameter = VisualThemeParameter.Serial;
        floor.environmentParameters.agentPerspective = AgentPerspective.ThirdPerson;
        floor.environmentParameters.allowedRoomTypes = AllowedRoomTypes.PlusPuzzle;
        floor.environmentParameters.allowedRoomModules = AllowedRoomModules.All;
        floor.environmentParameters.allowedFloorLayouts = AllowedFloorLayouts.PlusCircling;
        floor.environmentParameters.defaultTheme = VisualTheme.Ancient;
        floor.environmentParameters.use_ancient = true;
        floor.environmentParameters.use_moorish = true;
        floor.environmentParameters.use_industrial = true;
        floor.environmentParameters.use_modern = true;
        floor.environmentParameters.use_future = true;
    }

    private void UpdateEnvironmentParameters()
    {
        var lightType = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("lighting-type", (int)LightingType.Dynamic);
        if (Enum.IsDefined(typeof(LightingType), lightType))
        {
            floor.environmentParameters.lightingType = (LightingType) lightType;
        }
        else
        {
            Debug.Log("lighting-type outside of valid range. Using default value.");
        }

        var visTheme = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("visual-theme", (int)VisualThemeParameter.Serial);
        if (Enum.IsDefined(typeof(VisualThemeParameter), visTheme))
        {
            floor.environmentParameters.themeParameter = (VisualThemeParameter) visTheme;
            if (floor.environmentParameters.themeParameter.Equals(VisualThemeParameter.Random))
            {
                floor.environmentParameters.use_ancient = Convert.ToBoolean(Academy.Instance.EnvironmentParameters.GetWithDefault("use-ancient", 1.0f));
                floor.environmentParameters.use_moorish = Convert.ToBoolean(Academy.Instance.EnvironmentParameters.GetWithDefault("use-moorish", 1.0f));
                floor.environmentParameters.use_industrial = Convert.ToBoolean(Academy.Instance.EnvironmentParameters.GetWithDefault("use-industrial", 1.0f));
                floor.environmentParameters.use_modern = Convert.ToBoolean(Academy.Instance.EnvironmentParameters.GetWithDefault("use-modern", 1.0f));
                floor.environmentParameters.use_future = Convert.ToBoolean(Academy.Instance.EnvironmentParameters.GetWithDefault("use-future", 1.0f));
            }
        }
        else
        {
            Debug.Log("visual-theme outside of valid range. Using default value.");
        }
        
        var perspective = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("agent-perspective", (int)AgentPerspective.ThirdPerson);
        if (Enum.IsDefined(typeof(AgentPerspective), perspective))
        {
            floor.environmentParameters.agentPerspective = (AgentPerspective) perspective;
        }
        else
        {
            Debug.Log("agent-perspective outside of valid range. Using default value.");
        }
        
        var allowedRooms = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("allowed-rooms", (int)AllowedRoomTypes.PlusPuzzle);
        if (Enum.IsDefined(typeof(AllowedRoomTypes), allowedRooms))
        {
            floor.environmentParameters.allowedRoomTypes = (AllowedRoomTypes) allowedRooms;
        }
        else
        {
            Debug.Log("allowed-rooms outside of valid range. Using default value.");
        }
        
        var allowedModules = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("allowed-modules", (int)AllowedRoomModules.All);
        if (Enum.IsDefined(typeof(AllowedRoomModules), allowedModules))
        {
            floor.environmentParameters.allowedRoomModules = (AllowedRoomModules) allowedModules;
        }
        else
        {
            Debug.Log("allowed-modules outside of valid range. Using default value.");
        }
        
        var allowedFloors = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("allowed-floors", (int)AllowedFloorLayouts.PlusCircling);
        if (Enum.IsDefined(typeof(AllowedFloorLayouts), allowedFloors))
        {
            floor.environmentParameters.allowedFloorLayouts = (AllowedFloorLayouts) allowedFloors;
        }
        else
        {
            Debug.Log("allowed-floors outside of valid range. Using default value.");
        }
        
        var defaultTheme = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("default-theme", (int)VisualTheme.Ancient);
        if (Enum.IsDefined(typeof(VisualTheme), defaultTheme))
        {
            floor.environmentParameters.defaultTheme = (VisualTheme) defaultTheme;
        }
        else
        {
            Debug.Log("default-theme outside of valid range. Using default value.");
        }
    }

    public void ResetTower()
    {
        Debug.Log("Tower resetting");
        agentComponent.denseReward = Mathf.Clamp((int) Academy.Instance.EnvironmentParameters.GetWithDefault("dense-reward",0), 0, 1) != 0;
        if (InferenceOn)
        {
            EnableInference();
        }
        else
        {
            EnableTraining();
        }
        
        var towerSeed = Mathf.Clamp((int) Academy.Instance.EnvironmentParameters.GetWithDefault("tower-seed",-1), -1, MaxSeed);
        var totalFloors = Mathf.Clamp((int) Academy.Instance.EnvironmentParameters.GetWithDefault("total-floors", MaxFloors), 1, MaxFloors);
        var startingFloor = Mathf.Clamp((int) Academy.Instance.EnvironmentParameters.GetWithDefault("starting-floor",0), 0, totalFloors);
        
        UpdateEnvironmentParameters();
        
        if (totalFloors > 0 && totalFloors < MaxFloors)
        {
            floor.totalFloors = totalFloors;
        }
        if (startingFloor < floor.totalFloors && startingFloor >= 0)
        {
            floor.startingFloorNumber = startingFloor;
        }

        bool validSeed = towerSeed < MaxSeed;
        if (towerSeed != -1 && validSeed)
        {
            floor.fixedTower = true;
            floor.towerNumber = towerSeed;
        }
        else
        {
            floor.fixedTower = false;
        }
    }
}
