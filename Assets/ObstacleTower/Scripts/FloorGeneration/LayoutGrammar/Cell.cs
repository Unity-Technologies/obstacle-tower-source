using ObstacleTowerGeneration.MissionGraph;

namespace ObstacleTowerGeneration.LayoutGrammar
{
    /// <summary>
    /// The basic cell that construct the full map
    /// </summary>
    internal class Cell
    {
        /// <summary>
        /// The four doors that spawns from that node
        /// </summary>
        public readonly DoorType[] doorTypes;

        /// <summary>
        /// Constructor for the layout cell
        /// </summary>
        /// <param name="x">the current x</param>
        /// <param name="y">the current y</param>
        /// <param name="type">the cell type</param>
        /// <param name="node">the corresponding mission graph node</param>
        public Cell(int x, int y, CellType type, Node node)
        {
            this.x = x;
            this.y = y;
            this.type = type;
            this.node = node;
            doorTypes = new DoorType[4];
            parent = null;
        }

        /// <summary>
        /// The mission graph corresponding node, only works if the cell type is normal
        /// </summary>
        public Node node { set; get; }

        /// <summary>
        /// The type of the cell that define if there is a corresponding mission graph node for the current cell or not
        /// </summary>
        public CellType type { set; get; }

        /// <summary>
        /// The current x location of the cell
        /// </summary>
        public int x { set; get; }

        /// <summary>
        /// The current y location of the cell
        /// </summary>
        public int y { set; get; }

        /// <summary>
        /// The current parent cell that this cell was spawned from
        /// </summary>
        public Cell parent { set; get; }

        /// <summary>
        /// Create a copy of the current cell
        /// </summary>
        /// <returns>an exact copy of that cell</returns>
        public Cell Clone()
        {
            var result = new Cell(x, y, type, node);
            result.parent = parent;
            for (var i = 0; i < doorTypes.Length; i++) result.doorTypes[i] = doorTypes[i];

            return result;
        }

        /// <summary>
        /// transform direction vector to the correct index in the door array
        /// </summary>
        /// <param name="dirX">the x direction where -1 west and 1 east</param>
        /// <param name="dirY">the y direction where -1 north and 1 south</param>
        /// <returns>the index in the door array for the corresponding direction</returns>
        private int getDoorIndex(int dirX, int dirY)
        {
            if (dirX <= -1) return 0;

            if (dirX >= 1) return 1;

            if (dirY <= -1) return 2;

            if (dirY >= 1) return 3;

            return -1;
        }

        /// <summary>
        /// get the door type based on the input direction (east, west, north, south)
        /// </summary>
        /// <param name="dirX">the x direction where -1 west and 1 east</param>
        /// <param name="dirY">the y direction where -1 north and 1 south</param>
        /// <returns>the door type based on the direction</returns>
        public DoorType GetDoor(int dirX, int dirY)
        {
            return doorTypes[getDoorIndex(dirX, dirY)];
        }

        /// <summary>
        /// connect two cells together with a certain door type
        /// </summary>
        /// <param name="other">the other cell to be connect with</param>
        /// <param name="type">the type of door connection</param>
        /// <param name="bothWays">connect both cells</param>
        public void ConnectCells(Cell other, DoorType doorType, bool bothWays = true)
        {
            var dirX = x - other.x;
            var dirY = y - other.y;
            DoorType applyType = doorType;
            if (bothWays)
            {
                applyType = DoorType.Open;
            }

            doorTypes[getDoorIndex(dirX, dirY)] = applyType;
            if (bothWays)
            {
                other.ConnectCells(this, doorType, false);
            }
        }

        /// <summary>
        /// Get a unique identifier based on the current location
        /// </summary>
        /// <returns>a string of the current location as "x,y"</returns>
        public string GetLocationString()
        {
            return x + "," + y;
        }

        /// <summary>
        /// Draw the cell in a string correctly including the doors
        /// </summary>
        /// <returns>a string of how the room should look like in string rendering</returns>
        public override string ToString()
        {
            var result = "";
            var northDoor = '-';
            switch (doorTypes[3])
            {
                case DoorType.Open:
                    northDoor = ' ';
                    break;
                case DoorType.KeyLock:
                    northDoor = 'x';
                    break;
                case DoorType.LeverLock:
                case DoorType.PuzzleLock:
                    northDoor = 'v';
                    if (node != null && (node.type == NodeType.Puzzle ||
                                         node.type == NodeType.Lever))
                        northDoor = '^';

                    break;
            }

            var southDoor = '-';
            switch (doorTypes[2])
            {
                case DoorType.Open:
                    southDoor = ' ';
                    break;
                case DoorType.KeyLock:
                    southDoor = 'x';
                    break;
                case DoorType.LeverLock:
                case DoorType.PuzzleLock:
                    southDoor = '^';
                    if (node != null && (node.type == NodeType.Puzzle ||
                                         node.type == NodeType.Lever))
                        southDoor = 'v';

                    break;
            }

            var eastDoor = '|';
            switch (doorTypes[0])
            {
                case DoorType.Open:
                    eastDoor = ' ';
                    break;
                case DoorType.KeyLock:
                    eastDoor = 'x';
                    break;
                case DoorType.LeverLock:
                case DoorType.PuzzleLock:
                    eastDoor = '<';
                    if (node != null && (node.type == NodeType.Puzzle ||
                                         node.type == NodeType.Lever))
                        eastDoor = '>';

                    break;
            }

            var westDoor = '|';
            switch (doorTypes[1])
            {
                case DoorType.Open:
                    westDoor = ' ';
                    break;
                case DoorType.KeyLock:
                    westDoor = 'x';
                    break;
                case DoorType.LeverLock:
                case DoorType.PuzzleLock:
                    westDoor = '>';
                    if (node != null && (node.type == NodeType.Puzzle ||
                                         node.type == NodeType.Lever))
                        westDoor = '<';

                    break;
            }

            var roomType = 'C';
            if (type == CellType.Normal) roomType = (char) node.type;

            result += "+-" + northDoor + "-+" + "\n";
            result += "|   |" + "\n";
            result += westDoor + " " + roomType + " " + eastDoor + "\n";
            result += "|   |" + "\n";
            result += "+-" + southDoor + "-+";
            return result;
        }
    }
}
