using System;

/// <summary>
/// Structure containing relevant environment parameter information.
/// This is used to instantiate the floors of the tower according to the
/// prescribed reset parameters.
/// </summary>
[Serializable]
public struct EnvironmentParameters
{
    public bool trainMode;
    public LightingType lightingType;
    public AgentPerspective agentPerspective;
    public VisualThemeParameter themeParameter;
    public AllowedRoomModules allowedRoomModules;
    public AllowedRoomTypes allowedRoomTypes;
    public AllowedFloorLayouts allowedFloorLayouts;
    public VisualTheme defaultTheme;
    public bool use_ancient, use_moorish, use_industrial, use_modern, use_future;

    public bool Compare(EnvironmentParameters otherParams)
    {
        var equality = trainMode == otherParams.trainMode &&
                        lightingType == otherParams.lightingType && 
                        agentPerspective == otherParams.agentPerspective &&
                        themeParameter == otherParams.themeParameter &&
                        allowedRoomModules == otherParams.allowedRoomModules &&
                        allowedRoomTypes == otherParams.allowedRoomTypes &&
                        allowedFloorLayouts == otherParams.allowedFloorLayouts &&
                        defaultTheme == otherParams.defaultTheme &&
                        use_ancient == otherParams.use_ancient &&
                        use_moorish == otherParams.use_moorish &&
                        use_industrial == otherParams.use_industrial &&
                        use_modern == otherParams.use_modern &&
                        use_future == otherParams.use_future;
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
