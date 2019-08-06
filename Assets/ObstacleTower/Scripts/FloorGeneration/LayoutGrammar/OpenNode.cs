namespace ObstacleTowerGeneration.LayoutGrammar
{
    /// <summary>
    /// locations that are empty to place new cells
    /// </summary>
    class OpenNode
    {
        /// <summary>
        /// The current x location
        /// </summary>
        public int x { set; get; }

        /// <summary>
        /// the current y location
        /// </summary>
        public int y { set; get; }

        /// <summary>
        /// the parent cell that is nearby
        /// </summary>
        public Cell parent { set; get; }

        /// <summary>
        /// Constructor for the empty locations in the map
        /// </summary>
        /// <param name="x">the x location</param>
        /// <param name="y">the y location</param>
        /// <param name="parent">the parent cell that the empty location is connected to</param>
        public OpenNode(int x = 0, int y = 0, Cell parent = null)
        {
            this.x = x;
            this.y = y;
            this.parent = parent;
        }
    }
}
