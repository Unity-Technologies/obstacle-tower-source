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

    [Header("Default Reset Parameters")]
    [Range(-1, MaxSeed)]
    public int towerSeed = -1;
    [Range(0,99)]
    public int startingFloor = 0;
    [Range(1,100)]
    public int totalFloors = 100;
    public bool denseReward = true;
    public LightingType lightingType = LightingType.Dynamic;
    public VisualThemeParameter visualTheme = VisualThemeParameter.Serial;
    public AgentPerspective agentPerspective = AgentPerspective.ThirdPerson;
    public AllowedRoomTypes allowedRooms = AllowedRoomTypes.PlusPuzzle;
    public AllowedRoomModules allowedModules = AllowedRoomModules.All;
    public AllowedFloorLayouts allowedFloors = AllowedFloorLayouts.PlusCircling;
    public VisualTheme defaultTheme = VisualTheme.Ancient;
    [Header("Toogle single visual themes if Visual Theme is set to Random.")]
    public bool useAncient = true;
    public bool useMoorish = true;
    public bool useIndustrial = true;
    public bool useModern = true;
    public bool useFuture = true;

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
        var lightType = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("lighting-type", (int)lightingType);
        if (Enum.IsDefined(typeof(LightingType), lightType))
        {
            floor.environmentParameters.lightingType = (LightingType) lightType;
        }
        else
        {
            Debug.Log("lighting-type outside of valid range. Using default value.");
        }

        var visTheme = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("visual-theme", (int)visualTheme);
        if (Enum.IsDefined(typeof(VisualThemeParameter), visTheme))
        {
            floor.environmentParameters.themeParameter = (VisualThemeParameter) visTheme;
            if (floor.environmentParameters.themeParameter.Equals(VisualThemeParameter.Random))
            {
                floor.environmentParameters.use_ancient = Convert.ToBoolean(
                    Academy.Instance.EnvironmentParameters.GetWithDefault("use-ancient", Convert.ToSingle(useAncient)));
                floor.environmentParameters.use_moorish = Convert.ToBoolean(
                    Academy.Instance.EnvironmentParameters.GetWithDefault("use-moorish", Convert.ToSingle(useMoorish)));
                floor.environmentParameters.use_industrial = Convert.ToBoolean(
                    Academy.Instance.EnvironmentParameters.GetWithDefault("use-industrial", Convert.ToSingle(useIndustrial)));
                floor.environmentParameters.use_modern = Convert.ToBoolean(
                    Academy.Instance.EnvironmentParameters.GetWithDefault("use-modern", Convert.ToSingle(useModern)));
                floor.environmentParameters.use_future = Convert.ToBoolean(
                    Academy.Instance.EnvironmentParameters.GetWithDefault("use-future", Convert.ToSingle(useFuture)));
            }
        }
        else
        {
            Debug.Log("visual-theme outside of valid range. Using default value.");
        }
        
        var perspective = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("agent-perspective", (int)agentPerspective);
        if (Enum.IsDefined(typeof(AgentPerspective), perspective))
        {
            floor.environmentParameters.agentPerspective = (AgentPerspective) perspective;
        }
        else
        {
            Debug.Log("agent-perspective outside of valid range. Using default value.");
        }
        
        var allowedRoomTypes = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("allowed-rooms", (int)allowedRooms);
        if (Enum.IsDefined(typeof(AllowedRoomTypes), allowedRoomTypes))
        {
            floor.environmentParameters.allowedRoomTypes = (AllowedRoomTypes) allowedRoomTypes;
        }
        else
        {
            Debug.Log("allowed-rooms outside of valid range. Using default value.");
        }
        
        var allowedRoomModules = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("allowed-modules", (int)allowedModules);
        if (Enum.IsDefined(typeof(AllowedRoomModules), allowedRoomModules))
        {
            floor.environmentParameters.allowedRoomModules = (AllowedRoomModules) allowedRoomModules;
        }
        else
        {
            Debug.Log("allowed-modules outside of valid range. Using default value.");
        }
        
        var allowedFloorLayouts = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("allowed-floors", (int)allowedFloors);
        if (Enum.IsDefined(typeof(AllowedFloorLayouts), allowedFloorLayouts))
        {
            floor.environmentParameters.allowedFloorLayouts = (AllowedFloorLayouts)allowedFloorLayouts;
        }
        else
        {
            Debug.Log("allowed-floors outside of valid range. Using default value.");
        }
        
        var defaultVisualTheme = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("default-theme", (int)defaultTheme);
        if (Enum.IsDefined(typeof(VisualTheme), defaultVisualTheme))
        {
            floor.environmentParameters.defaultTheme = (VisualTheme)defaultVisualTheme;
        }
        else
        {
            Debug.Log("default-theme outside of valid range. Using default value.");
        }
    }

    public void ResetTower()
    {
        Debug.Log("Tower resetting");
        agentComponent.denseReward = Mathf.Clamp((int)Academy.Instance.EnvironmentParameters.GetWithDefault("dense-reward", Convert.ToInt32(denseReward)), 0, 1) != 0;
        if (InferenceOn)
        {
            EnableInference();
        }
        else
        {
            EnableTraining();
        }
        
        var seed = Mathf.Clamp((int)Academy.Instance.EnvironmentParameters.GetWithDefault("tower-seed", towerSeed), -1, MaxSeed);
        var floors = Mathf.Clamp((int)Academy.Instance.EnvironmentParameters.GetWithDefault("total-floors", totalFloors), 1, MaxFloors);
        var start = Mathf.Clamp((int)Academy.Instance.EnvironmentParameters.GetWithDefault("starting-floor", startingFloor), 0, floors);
        
        UpdateEnvironmentParameters();
        
        if (floors > 0 && floors < MaxFloors)
        {
            floor.totalFloors = floors;
        }
        if (start < floor.totalFloors && start >= 0)
        {
            floor.startingFloorNumber = start;
        }

        bool validSeed = seed < MaxSeed;
        if (seed != -1 && validSeed)
        {
            floor.fixedTower = true;
            floor.towerNumber = seed;
        }
        else
        {
            floor.fixedTower = false;
        }
    }
}
