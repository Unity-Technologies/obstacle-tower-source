namespace ObstacleTowerGeneration.LayoutGrammar
{
    /// <summary>
    /// Different types of doors for each cell where each cell have 4 doors
    /// </summary>
    enum DoorType{
        Open = ' ',
        KeyLock = 'x',
        LeverLock = 'v',
        PuzzleLock = 'z',
        Start = 's',
        Exit = 'e',
        OneWay = 'o'
    }
}
