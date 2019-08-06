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
        SetIsInference(true);
        agentComponent.SetInference();
        Time.captureFramerate = 0;
    }

    private void EnableTraining()
    {
        SetIsInference(false);
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
        if (Enum.IsDefined(typeof(LightingType), (int)resetParameters["lighting-type"]))
        {
            floor.environmentParameters.lightingType = (LightingType) resetParameters["lighting-type"];
        }
        else
        {
            Debug.Log("lighting-type outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(VisualThemeParameter), (int)resetParameters["visual-theme"]))
        {
            floor.environmentParameters.themeParameter = (VisualThemeParameter) resetParameters["visual-theme"];
        }
        else
        {
            Debug.Log("visual-theme outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(AgentPerspective), (int)resetParameters["agent-perspective"]))
        {
            floor.environmentParameters.agentPerspective = (AgentPerspective) resetParameters["agent-perspective"];
        }
        else
        {
            Debug.Log("agent-perspective outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(AllowedRoomTypes), (int)resetParameters["allowed-rooms"]))
        {
            floor.environmentParameters.allowedRoomTypes = (AllowedRoomTypes) resetParameters["allowed-rooms"];
        }
        else
        {
            Debug.Log("allowed-rooms outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(AllowedRoomModules), (int)resetParameters["allowed-modules"]))
        {
            floor.environmentParameters.allowedRoomModules = (AllowedRoomModules) resetParameters["allowed-modules"];
        }
        else
        {
            Debug.Log("allowed-modules outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(AllowedFloorLayouts), (int)resetParameters["allowed-modules"]))
        {
            floor.environmentParameters.allowedFloorLayouts = (AllowedFloorLayouts) resetParameters["allowed-floors"];
        }
        else
        {
            Debug.Log("allowed-floors outside of valid range. Using default value.");
        }
        
        if (Enum.IsDefined(typeof(VisualTheme), (int)resetParameters["default-theme"]))
        {
            floor.environmentParameters.defaultTheme = (VisualTheme) resetParameters["default-theme"];
        }
        else
        {
            Debug.Log("default-theme outside of valid range. Using default value.");
        }
    }

    public override void AcademyReset()
    {
        Debug.Log("Academy resetting");
        agentComponent.denseReward = Mathf.Clamp((int) resetParameters["dense-reward"], 0, 1) != 0;
        if (GetIsInference())
        {
            EnableInference();
        }
        else
        {
            EnableTraining();
        }
        
        var towerSeed = Mathf.Clamp((int) resetParameters["tower-seed"], -1, MaxSeed);
        var totalFloors = Mathf.Clamp((int) resetParameters["total-floors"], 1, MaxFloors);
        var startingFloor = Mathf.Clamp((int) resetParameters["starting-floor"], 0, totalFloors);
        
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
