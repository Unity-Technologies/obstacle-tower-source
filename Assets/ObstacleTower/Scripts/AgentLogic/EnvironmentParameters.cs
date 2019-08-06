using System;

/// <summary>
/// Structure containing relevant environment parameter information.
/// This is used to instantiate the floors of the tower according to the
/// prescribed reset parameters.
/// </summary>
[Serializable]
public struct EnvironmentParameters
{
    public LightingType lightingType;
    public AgentPerspective agentPerspective;
    public VisualThemeParameter themeParameter;
    public AllowedRoomModules allowedRoomModules;
    public AllowedRoomTypes allowedRoomTypes;
    public AllowedFloorLayouts allowedFloorLayouts;
    public VisualTheme defaultTheme;

    public bool Compare(EnvironmentParameters otherParams)
    {
        var equality = lightingType == otherParams.lightingType && 
                        agentPerspective == otherParams.agentPerspective &&
                        themeParameter == otherParams.themeParameter &&
                        allowedRoomModules == otherParams.allowedRoomModules &&
                        allowedRoomTypes == otherParams.allowedRoomTypes &&
                        allowedFloorLayouts == otherParams.allowedFloorLayouts &&
                        defaultTheme == otherParams.defaultTheme;
        return equality;
    }
}

public enum AgentPerspective
{
    FirstPerson,
    ThirdPerson
}

public enum LightingType
{
    Fixed,
    Dynamic,
    Extreme
}

public enum VisualThemeParameter
{
    One,
    Serial,
    Random
}

public enum AllowedRoomModules
{
    None,
    Easy,
    All
}

public enum AllowedRoomTypes
{
    Normal,
    PlusKey,
    PlusPuzzle
}

public enum AllowedFloorLayouts
{
    Linear,
    PlusBranching,
    PlusCircling
}
