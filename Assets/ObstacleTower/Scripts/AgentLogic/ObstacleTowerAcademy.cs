using System;
using UnityEngine;
using MLAgents;
using Debug = UnityEngine.Debug;
using UnityEngine.Rendering;


/// <summary>
/// Academy for Obstacle Tower.
/// Responsible for reading relevant reset parameters, and evaluation logic.
/// </summary>
public class ObstacleTowerAcademy : Academy
{
    public FloorBuilder floor;

    public const int MaxSeed = 99999;
    public const int MaxFloors = 100;
    
    private ObstacleTowerAgent agentComponent;

    [Header("Reset Parameters")]
    [Tooltip("Sets the seed used to generate the tower. -1 corresponds to a random tower on every reset() call.")]
    public int towerSeed = -1;
    [Tooltip("Sets the starting floor for the agent on reset().")]
    public int startingFloor = 0;
    [Tooltip("Sets the maximum number of possible floors in the tower.")]
    public int totalFloors = 100;
    [Tooltip("Whether to use the sparse (0) or dense (1) reward function.")]
    public int denseReward = 1;
    [Tooltip("Whether to use no realtime light (0), a single realtime light with minimal color variations (1), or a realtime light with large color variations (2).")]
    public int lightningType = 1;
    [Tooltip("Whether to use only the default-theme (0), the normal ordering or themes (1), or a random theme every floor (2).")]
    public int visualTheme = 1;
    [Tooltip("Whether to use first-person (0) or third-person (1) perspective for the agent.")]
    public int agentPerspective = 1;
    [Tooltip("Whether to use only normal rooms (0), normal and key rooms (1), or normal, key, and puzzle rooms (2).")]
    public int allowedRooms = 2;
    [Tooltip("Whether to fill rooms with no modules (0), only easy modules (1), or the full range of modules (2).")]
    public int allowedModules = 2;
    [Tooltip("Whether to include only straightforward floor layouts (0), layouts that include branching (1), or layouts that include branching and circling (2).")]
    public int allowedFloors = 2;
    [Tooltip("Whether to set the default theme to Ancient (0), Moorish (1), Industrial (2), Modern (3), or Future (4).")]
    public int defaultTheme = 0;

    public override void InitializeAcademy()
    {
        floor.environmentParameters = new EnvironmentParameters();
        agentComponent = FindObjectOfType<ObstacleTowerAgent>();
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
    }
    
    private void EnableInference()
    {
        agentComponent.SetInference();
        Time.captureFramerate = 0;
    }

    private void EnableTraining()
    {
        agentComponent.SetTraining();
        Time.captureFramerate = 60;
    }

    private void UpdateEnvironmentParameters()
    {
        if (Enum.IsDefined(typeof(LightingType), (int)FloatProperties.GetPropertyWithDefault("lightning-type", lightningType)))
        {
            floor.environmentParameters.lightingType = (LightingType)FloatProperties.GetPropertyWithDefault("lightning-type", lightningType);
        }
        else
        {
            Debug.Log("lighting-type outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(VisualThemeParameter), (int)FloatProperties.GetPropertyWithDefault("visual-theme", visualTheme)))
        {
            floor.environmentParameters.themeParameter = (VisualThemeParameter)FloatProperties.GetPropertyWithDefault("visual-theme", visualTheme);
        }
        else
        {
            Debug.Log("visual-theme outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(AgentPerspective), (int)FloatProperties.GetPropertyWithDefault("agent-perspective", agentPerspective)))
        {
            floor.environmentParameters.agentPerspective = (AgentPerspective)FloatProperties.GetPropertyWithDefault("agent-perspective", agentPerspective);
        }
        else
        {
            Debug.Log("agent-perspective outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(AllowedRoomTypes), (int)FloatProperties.GetPropertyWithDefault("allowed-rooms", allowedRooms)))
        {
            floor.environmentParameters.allowedRoomTypes = (AllowedRoomTypes)FloatProperties.GetPropertyWithDefault("allowed-rooms", allowedRooms);
        }
        else
        {
            Debug.Log("allowed-rooms outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(AllowedRoomModules), (int)FloatProperties.GetPropertyWithDefault("allowed-modules", allowedModules)))
        {
            floor.environmentParameters.allowedRoomModules = (AllowedRoomModules)FloatProperties.GetPropertyWithDefault("allowed-modules", allowedModules);
        }
        else
        {
            Debug.Log("allowed-modules outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(AllowedFloorLayouts), (int)FloatProperties.GetPropertyWithDefault("allowed-floors", allowedFloors)))
        {
            floor.environmentParameters.allowedFloorLayouts = (AllowedFloorLayouts)FloatProperties.GetPropertyWithDefault("allowed-floors", allowedFloors);

        }
        else
        {
            Debug.Log("allowed-floors outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(VisualTheme), (int)FloatProperties.GetPropertyWithDefault("default-theme", defaultTheme)))
        {
            floor.environmentParameters.defaultTheme = (VisualTheme)FloatProperties.GetPropertyWithDefault("default-theme", defaultTheme);
        }
        else
        {
            Debug.Log("default-theme outside of valid range. Using default value.");
        }

        var seed = Mathf.Clamp((int)FloatProperties.GetPropertyWithDefault("tower-seed", towerSeed), -1, MaxSeed);
        var floors = Mathf.Clamp((int)FloatProperties.GetPropertyWithDefault("total-floors", totalFloors), 1, MaxFloors);
        var start = Mathf.Clamp((int)FloatProperties.GetPropertyWithDefault("starting-floor", startingFloor), 0, floors);

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

    public override void AcademyReset()
    {
        Debug.Log("Academy resetting");
        agentComponent.denseReward = Mathf.Clamp((int)FloatProperties.GetPropertyWithDefault("dense-reward", denseReward), 0, 1) != 0;

        // If the communicator to Python is not on, inference mode is used
        if (!IsCommunicatorOn)
        {
            EnableInference();
        }
        else
        {
            EnableTraining();
        }
        
        UpdateEnvironmentParameters();
    }
}
