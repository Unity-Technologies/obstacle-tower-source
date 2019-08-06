
namespace ObstacleTowerGeneration.MissionGraph{
    /// <summary>
    /// Different types of nodes in the mission graph
    /// </summary>
    public enum NodeType
    {
        None= ' ',
        Normal = 'N',
        Lock = 'L',
        Key = 'K',
        Lever = 'V',
        Puzzle = 'P',
        Any = ' ',
        Start = 'S',
        End = 'E',
        Basement = 'B',
        Connection = 'C'
    }
}
