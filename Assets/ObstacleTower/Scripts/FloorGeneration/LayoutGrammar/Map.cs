using System;
using System.Collections.Generic;
using ObstacleTowerGeneration.MissionGraph;

namespace ObstacleTowerGeneration.LayoutGrammar
{
    /// <summary>
    /// The map layout class
    /// </summary>
    internal class Map
    {
        /// <summary>
        /// A list of all the possible direction to connect cells
        /// </summary>
        /// <value></value>
        private readonly List<int[]> directions = new List<int[]>
        {
            new[] {-1, 0}, new[] {1, 0},
            new[] {0, -1}, new[] {0, 1}
        };
        /// <summary>
        /// A dictionary of all possible openings from each location
        /// </summary>
        private readonly Dictionary<string, List<OpenNode>> openLocations;
        /// <summary>
        /// random variable used to select cells randomly for expansions
        /// </summary>
        private readonly Random random;
        /// <summary>
        /// A dictionary of all used x,y locations
        /// </summary>
        private readonly Dictionary<string, Cell> usedSpaces;

        /// <summary>
        /// Constructor that creates an empty layout
        /// </summary>
        /// <param name="random">the random variable that is used in generation</param>
        public Map(Random random)
        {
            this.random = random;
            usedSpaces = new Dictionary<string, Cell>();
            openLocations = new Dictionary<string, List<OpenNode>>();
        }

        /// <summary>
        /// Start the map by adding the first cell that correspond to the starting node of the mission graph
        /// </summary>
        /// <param name="node">the starting cell that has only to be connected randomly from one direction</param>
        public void InitializeCell(Node node)
        {
            var start = new Cell(0, 0, CellType.Normal, node);
            usedSpaces.Add(start.GetLocationString(), start);
            openLocations.Add("0,0", new List<OpenNode>());
            var randDir = directions[random.Next(directions.Count)];
            openLocations["0,0"].Add(new OpenNode(randDir[0], randDir[1], start));
        }

        /// <summary>
        /// Get an empty location that has a specific access level and connected to a certain node
        /// </summary>
        /// <param name="parentID">the node id that need to be connected to</param>
        /// <param name="accessLevel">the access level that the cell need to be at</param>
        /// <returns>the empty location that has a certain parent id and access level</returns>
        private OpenNode GetWorkingLocation(int parentID, int accessLevel)
        {
            if (!openLocations.ContainsKey(parentID + "," + accessLevel)) return null;

            Helper.ShuffleList(random, openLocations[parentID + "," + accessLevel]);
            OpenNode selected = null;
            foreach (var loc in openLocations[parentID + "," + accessLevel])
                if (!usedSpaces.ContainsKey(loc.x + "," + loc.y))
                {
                    selected = loc;
                    break;
                }

            return selected;
        }

        /// <summary>
        /// Adding a new cell in the map and modifying the used spaces and open spaces
        /// </summary>
        /// <param name="c">the new cell that is being added</param>
        /// <param name="nextAccess">the new access level based on that cell</param>
        /// <param name="parentID">the parent id that new cell</param>
        private void AddNewNode(Cell c, int nextAccess, int parentID)
        {
            usedSpaces.Add(c.GetLocationString(), c);
            if (!openLocations.ContainsKey(parentID + "," + nextAccess))
                openLocations.Add(parentID + "," + nextAccess, new List<OpenNode>());

            foreach (var dir in directions)
            {
                var newX = c.x + dir[0];
                var newY = c.y + dir[1];
                if (!usedSpaces.ContainsKey(newX + "," + newY))
                    openLocations[parentID + "," + nextAccess].Add(new OpenNode(newX, newY, c));
            }
        }

        /// <summary>
        /// Get the cell that has a certain mission graph node id
        /// </summary>
        /// <param name="id">the id of the mission graph node</param>
        /// <returns>the cell that have a mission graph node id</returns>
        public Cell GetCell(int id)
        {
            foreach (var c in usedSpaces.Values)
                if (c.node != null && c.node.id == id)
                    return c;

            return null;
        }

        /// <summary>
        /// Get a path between the "from" cell to the "to" cell without changing the access level
        /// </summary>
        /// <param name="from">the starting cell</param>
        /// <param name="to">the ending cell</param>
        /// <param name="accessLevel">the access level of the successor cells in the path</param>
        /// <returns>a list of (x,y) locations that connect between "from" cell to "to" cell</returns>
        public List<int[]> GetDungeonPath(Cell from, Cell to, int accessLevel)
        {
            var queue = new List<TreeNode>();
            queue.Add(new TreeNode(from.x, from.y, null, to.x, to.y));
            var visited = new HashSet<string>();
            while (queue.Count > 0)
            {
                queue.Sort();
                var current = queue[0];
                queue.Remove(current);
                if (current.x == to.x && current.y == to.y) return current.GetPath();

                if (from.parent != null && current.x == from.parent.x && current.y == from.parent.y) continue;

                if (visited.Contains(current.x + "," + current.y)) continue;

                if (!usedSpaces.ContainsKey(current.x + "," + current.y)) continue;

                if (usedSpaces[current.x + "," + current.y].type == CellType.Connection) continue;

                if (usedSpaces[current.x + "," + current.y].node.accessLevel != accessLevel) continue;

                visited.Add(current.x + "," + current.y);
                foreach (var dir in directions)
                    if (usedSpaces[current.x + "," + current.y].GetDoor(-dir[0], -dir[1]) != 0)
                    {
                        var newX = current.x + dir[0];
                        var newY = current.y + dir[1];
                        queue.Add(new TreeNode(newX, newY, current, to.x, to.y));
                    }
            }

            return new List<int[]>();
        }

        /// <summary>
        /// Get a list of all open spaces that are needed to be transformed to cells to maintain connection between "from" cell to "to" cell
        /// </summary>
        /// <param name="from">the starting cell</param>
        /// <param name="to">the ending cell</param>
        /// <param name="maxIterations">the number of times that algorithm should try before failing</param>
        /// <returns>the empty locations that need to be converted to "conncet" cells</returns>
        private List<int[]> GetConnectionPoints(Cell from, Cell to, int maxIterations)
        {
            var accessLevel = Math.Min(from.node.accessLevel, to.node.accessLevel);
            var queue = new List<TreeNode>();
            foreach (var dir in directions)
            {
                var newX = from.x + dir[0];
                var newY = from.y + dir[1];
                queue.Add(new TreeNode(newX, newY, new TreeNode(from.x, from.y, null, to.x, to.y), to.x, to.y));
            }

            var visited = new HashSet<string>();
            visited.Add(from.x + "," + from.y);
            while (queue.Count > 0)
            {
                queue.Sort();
                var current = queue[0];
                queue.Remove(current);
                if (current.x == to.x && current.y == to.y) return current.GetPath();

                if (usedSpaces.ContainsKey(current.x + "," + current.y) &&
                    usedSpaces[current.x + "," + current.y].type == CellType.Normal)
                {
                    if (usedSpaces[current.x + "," + current.y].node.accessLevel > accessLevel) continue;

                    var dungeonPath =
                        GetDungeonPath(usedSpaces[current.x + "," + current.y], to, accessLevel);
                    if (dungeonPath.Count > 0) return current.GetPath();
                }

                if (from.parent != null && current.x == from.parent.x && current.y == from.parent.y) continue;

                if (visited.Contains(current.x + "," + current.y)) continue;

                if (visited.Count > maxIterations) return new List<int[]>();

                visited.Add(current.x + "," + current.y);
                foreach (var dir in directions)
                {
                    var newX = current.x + dir[0];
                    var newY = current.y + dir[1];
                    queue.Add(new TreeNode(newX, newY, current, to.x, to.y));
                }
            }

            return new List<int[]>();
        }

        /// <summary>
        /// Create the "connect" cells to connect "from" cell to "to" cell
        /// </summary>
        /// <param name="from">the starting cell</param>
        /// <param name="to">the end cell</param>
        /// <param name="maxIterations">the maximum number of trials before failing</param>
        /// <returns>True if the connection succeeded and False otherwise</returns>
        public bool MakeConnection(Cell from, Cell to, int maxIterations)
        {
            var points = GetConnectionPoints(from, to, maxIterations);
            if (points.Count == 0) return false;

            foreach (var p in points)
                if (!usedSpaces.ContainsKey(p[0] + "," + p[1]))
                {
                    var currentCell = new Cell(p[0], p[1], CellType.Connection, null);
                    usedSpaces.Add(currentCell.GetLocationString(), currentCell);
                }

            for (var i = 1; i < points.Count; i++)
            {
                var p = points[i];
                var previousCell = usedSpaces[points[i - 1][0] + "," + points[i - 1][1]];
                var currentCell = usedSpaces[p[0] + "," + p[1]];
                var door = DoorType.OneWay;
                if (previousCell.node != null)
                    if (previousCell.node.type == NodeType.Lever)
                        door = DoorType.LeverLock;

                if (currentCell.GetDoor(currentCell.x - previousCell.x, currentCell.y - previousCell.y) == 0)
                    currentCell.ConnectCells(previousCell, door);
            }

            return true;
        }

        /// <summary>
        /// Add a new cell to the layout that correspond to a certain node in the mission graph
        /// </summary>
        /// <param name="node">corresponding node in the mission graph</param>
        /// <param name="parentID">the id of the parent that the new cell should be connected to</param>
        /// <returns>True if it succeed and False otherwise</returns>
        public bool AddCell(Node node, int parentID)
        {
            if (node.type == NodeType.Lock)
            {
                var selected = GetWorkingLocation(parentID, node.accessLevel - 1);
                if (selected == null) return false;

                var newCell = new Cell(selected.x, selected.y, CellType.Normal, node);
                newCell.ConnectCells(selected.parent, DoorType.KeyLock);
                newCell.parent = selected.parent;
                AddNewNode(newCell, node.accessLevel, node.id);
            }
            else if (node.type == NodeType.Puzzle)
            {
                var selected = GetWorkingLocation(parentID, node.accessLevel);
                if (selected == null) return false;

                var newCell = new Cell(selected.x, selected.y, CellType.Normal, node);
                newCell.ConnectCells(selected.parent, DoorType.Open);
                newCell.parent = selected.parent;
                AddNewNode(newCell, node.accessLevel + 1, node.id);
            }
            else if (node.type == NodeType.Lever)
            {
                var selected = GetWorkingLocation(parentID, node.accessLevel);
                if (selected == null) return false;

                var newCell = new Cell(selected.x, selected.y, CellType.Normal, node);
                newCell.ConnectCells(selected.parent, DoorType.Open);
                newCell.parent = selected.parent;
                usedSpaces.Add(newCell.GetLocationString(), newCell);
            }
            else
            {
                var selected = GetWorkingLocation(parentID, node.accessLevel);
                if (selected == null) return false;

                var newCell = new Cell(selected.x, selected.y, CellType.Normal, node);
                if (selected.parent.node.type == NodeType.Puzzle)
                    newCell.ConnectCells(selected.parent, DoorType.PuzzleLock);
                else if (selected.parent.node.type == NodeType.Lever)
                    newCell.ConnectCells(selected.parent, DoorType.LeverLock);
                else
                    newCell.ConnectCells(selected.parent, DoorType.Open);

                if (node.GetChildren().Count == 0)
                    usedSpaces.Add(newCell.GetLocationString(), newCell);
                else
                    AddNewNode(newCell, node.accessLevel, parentID);

                newCell.parent = selected.parent;
            }

            return true;
        }

        /// <summary>
        /// return a 2D array of cells where the cells are placed in a grid
        /// </summary>
        /// <returns>2D array of the cells</returns>
        public Cell[,] Get2DMap()
        {
            var minX = 0;
            var maxX = 0;
            var minY = 0;
            var maxY = 0;
            foreach (var c in usedSpaces.Values)
            {
                if (c.x < minX) minX = c.x;

                if (c.y < minY) minY = c.y;

                if (c.x > maxX) maxX = c.x;

                if (c.y > maxY) maxY = c.y;
            }

            var result = new Cell[maxX - minX + 1, maxY - minY + 1];
            foreach (var c in usedSpaces.Values)
            {
                var clone = c.Clone();
                clone.x = c.x - minX;
                clone.y = c.y - minY;
                result[clone.x, clone.y] = clone;
            }

            return result;
        }

        /// <summary>
        /// Get a string representation of the current layout
        /// </summary>
        /// <returns>a string of the current map layout</returns>
        public override string ToString()
        {
            var nullCell = "     \n     \n     \n     \n     ";
            var result = "";
            var result2D = Get2DMap();
            for (var y = 0; y < result2D.GetLength(1); y++)
            {
                var parts = new string[5];
                for (var x = 0; x < result2D.GetLength(0); x++)
                {
                    var temp = Helper.SplitLines(nullCell);
                    if (result2D[x, y] != null)
                        temp = Helper.SplitLines(result2D[x, y].ToString());

                    for (var i = 0; i < parts.Length; i++) parts[i] += temp[i];
                }

                for (var i = 0; i < parts.Length; i++) result += parts[i] + "\n";
            }

            return result;
        }
    }
}
