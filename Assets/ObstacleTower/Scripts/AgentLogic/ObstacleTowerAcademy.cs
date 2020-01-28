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

    private void SetDefaultEnvironmentParameters()
    {
        floor.environmentParameters.lightingType = LightingType.Dynamic;
        floor.environmentParameters.themeParameter = VisualThemeParameter.Serial;
        floor.environmentParameters.agentPerspective = AgentPerspective.ThirdPerson;
        floor.environmentParameters.allowedRoomTypes = AllowedRoomTypes.PlusPuzzle;
        floor.environmentParameters.allowedRoomModules = AllowedRoomModules.All;
        floor.environmentParameters.allowedFloorLayouts = AllowedFloorLayouts.PlusCircling;
        floor.environmentParameters.defaultTheme = VisualTheme.Ancient;
    }

    private void UpdateEnvironmentParameters()
    {
        if (Enum.IsDefined(typeof(LightingType), (int)FloatProperties.GetPropertyWithDefault("lightning-type", 1)))
        {
            floor.environmentParameters.lightingType = (LightingType)FloatProperties.GetPropertyWithDefault("lightning-type", 1);
        }
        else
        {
            Debug.Log("lighting-type outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(VisualThemeParameter), (int)FloatProperties.GetPropertyWithDefault("visual-theme", 1)))
        {
            floor.environmentParameters.themeParameter = (VisualThemeParameter)FloatProperties.GetPropertyWithDefault("visual-theme", 1);
        }
        else
        {
            Debug.Log("visual-theme outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(AgentPerspective), (int)FloatProperties.GetPropertyWithDefault("agent-perspective", 1)))
        {
            floor.environmentParameters.agentPerspective = (AgentPerspective)FloatProperties.GetPropertyWithDefault("agent-perspective", 1);
        }
        else
        {
            Debug.Log("agent-perspective outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(AllowedRoomTypes), (int)FloatProperties.GetPropertyWithDefault("allowed-rooms", 2)))
        {
            floor.environmentParameters.allowedRoomTypes = (AllowedRoomTypes)FloatProperties.GetPropertyWithDefault("allowed-rooms", 2);
        }
        else
        {
            Debug.Log("allowed-rooms outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(AllowedRoomModules), (int)FloatProperties.GetPropertyWithDefault("allowed-modules", 2)))
        {
            floor.environmentParameters.allowedRoomModules = (AllowedRoomModules)FloatProperties.GetPropertyWithDefault("allowed-modules", 2);
        }
        else
        {
            Debug.Log("allowed-modules outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(AllowedFloorLayouts), (int)FloatProperties.GetPropertyWithDefault("allowed-modules", 2)))
        {
            floor.environmentParameters.allowedFloorLayouts = (AllowedFloorLayouts)FloatProperties.GetPropertyWithDefault("allowed-floors", 2);

        }
        else
        {
            Debug.Log("allowed-floors outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(VisualTheme), (int)FloatProperties.GetPropertyWithDefault("default-theme", 0)))
        {
            floor.environmentParameters.defaultTheme = (VisualTheme)FloatProperties.GetPropertyWithDefault("default-theme", 0);
        }
        else
        {
            Debug.Log("default-theme outside of valid range. Using default value.");
        }
    }

    public override void AcademyReset()
    {
        Debug.Log("Academy resetting");
        agentComponent.denseReward = Mathf.Clamp((int)FloatProperties.GetPropertyWithDefault("dense-reward", 1), 0, 1) != 0;

        // If the communicator to Python is not on, inference mode is used
        if (!IsCommunicatorOn)
        {
            EnableInference();
        }
        else
        {
            EnableTraining();
        }

        var towerSeed = Mathf.Clamp((int)FloatProperties.GetPropertyWithDefault("tower-seed", -1), -1, MaxSeed);
        var totalFloors = Mathf.Clamp((int)FloatProperties.GetPropertyWithDefault("total-floors", 100), 1, MaxFloors);
        var startingFloor = Mathf.Clamp((int)FloatProperties.GetPropertyWithDefault("starting-floor", 0), 0, totalFloors);
        
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
