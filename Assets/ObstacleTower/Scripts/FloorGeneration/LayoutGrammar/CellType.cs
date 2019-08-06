namespace ObstacleTowerGeneration.LayoutGrammar{
    /// <summary>
    /// Different type of cells in the layout
    /// </summary>
    enum CellType{
        ///Normal cells that have a corresponding node in the mission graph
        Normal = 'N',
        ///Connecting cells that are added to insure connectivity in the layout similar to the mission graph connectivity
        Connection = 'C'
    }
}
