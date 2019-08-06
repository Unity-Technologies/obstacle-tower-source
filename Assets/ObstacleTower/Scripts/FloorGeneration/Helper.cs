using System.Collections.Generic;
using System;
using ObstacleTowerGeneration.MissionGraph;
using ObstacleTowerGeneration.LayoutGrammar;

namespace ObstacleTowerGeneration
{
    /// <summary>
    /// A helper class that provide a group of useful function used during the layout generation
    /// </summary>
    static class Helper
    {
        /// <summary>
        /// make a copy of the input list and return it
        /// </summary>
        /// <param name="list">the list needed to be copied</param>
        /// <returns>a clone of the input list</returns>
        public static string[] SplitLines(string s)
        {
            return s.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );
        }

        private static List<T> CopyList<T>(List<T> list)
        {
            List<T> clone = new List<T>();
            foreach (T v in list)
            {
                clone.Add(v);
            }

            return clone;
        }
        
        /// <summary>
        /// Checks if you can still finish the dungeon and 
        /// the player won't get stuck if they used the wrong key with the wrong door
        /// </summary>
        /// <param name="graph">the current mission graph</param>
        /// <param name="root">the starting node in the graph</param>
        /// <param name="visited">used in the recurssion to keep track of the visited nodes in the graph</param>
        /// <returns>true if the player can reach the exist using that mission graph and false otherwise</returns>
        public static bool CheckIsSolvable(Graph graph, Node root, HashSet<Node> visited = null)
        {
            List<Node> queue = new List<Node>();
            List<Node> locks = new List<Node>();
            if (visited == null)
            {
                visited = new HashSet<Node>();
            }

            visited.Add(root);
            foreach (Node c in root.GetChildren())
            {
                queue.Add(c);
            }

            int keys = 0;
            while (queue.Count > 0)
            {
                Node current = queue[0];
                queue.RemoveAt(0);
                if (visited.Contains(current))
                {
                    continue;
                }

                if (current.type == NodeType.Lock)
                {
                    locks.Add(current);
                }
                else
                {
                    if (current.type == NodeType.End)
                    {
                        return true;
                    }
                    else if (current.type == NodeType.Key)
                    {
                        keys += 1;
                    }

                    visited.Add(current);
                    foreach (Node c in current.GetChildren())
                    {
                        queue.Add(c);
                    }
                }
            }

            int requiredKeys = 0;
            foreach (Node l in locks)
            {
                HashSet<Node> newVisited = new HashSet<Node>();
                foreach (Node v in visited)
                {
                    newVisited.Add(v);
                }

                if (!CheckIsSolvable(graph, l, newVisited))
                {
                    requiredKeys += 1;
                }
            }

            if (requiredKeys < keys)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the current generated layout is solvable and 
        /// the player won't get stuck because of using wrong key with a wrong door
        /// </summary>
        /// <param name="map">the generated layout from the mission graph</param>
        /// <param name="start">the starting cell in the generated layout</param>
        /// <param name="visited">used in the recurssion to know which cells in the layout has been visited</param>
        /// <returns>true if the player can reach the exist using that layout and false otherwise</returns>
        public static bool CheckIsSolvable(Cell[,] map, Cell start, HashSet<Cell> visited = null)
        {
            List<int[]> directions = new List<int[]>
            {
                new[] {-1, 0}, new[] {1, 0},
                new[] {0, -1}, new[] {0, 1}
            };

            List<Cell> queue = new List<Cell>();
            List<Cell> locks = new List<Cell>();
            if (visited == null)
            {
                visited = new HashSet<Cell>();
            }

            visited.Add(start);
            if (start.type == CellType.Normal && start.node.type != NodeType.Lever)
            {
                foreach (int[] dir in directions)
                {
                    int[] newPos = {start.x + dir[0], start.y + dir[1]};
                    if (newPos[0] < 0 || newPos[1] < 0 || newPos[0] >= map.GetLength(0) ||
                        newPos[1] >= map.GetLength(1))
                    {
                        continue;
                    }

                    if (map[newPos[0], newPos[1]] != null)
                    {
                        queue.Add(map[newPos[0], newPos[1]]);
                    }
                }
            }

            int keys = 0;
            while (queue.Count > 0)
            {
                Cell current = queue[0];
                queue.RemoveAt(0);
                if (visited.Contains(current))
                {
                    continue;
                }

                if (current.type == CellType.Normal && current.node.type == NodeType.Lock)
                {
                    locks.Add(current);
                }
                else
                {
                    if (current.type == CellType.Normal && current.node.type == NodeType.End)
                    {
                        return true;
                    }
                    else if (current.type == CellType.Normal && current.node.type == NodeType.Key)
                    {
                        keys += 1;
                    }

                    visited.Add(current);
                    if (current.type == CellType.Normal && current.node.type != NodeType.Lever)
                    {
                        foreach (int[] dir in directions)
                        {
                            int[] newPos = new int[] {current.x + dir[0], current.y + dir[1]};
                            if (newPos[0] < 0 || newPos[1] < 0 || newPos[0] >= map.GetLength(0) ||
                                newPos[1] >= map.GetLength(1))
                            {
                                continue;
                            }

                            if (map[newPos[0], newPos[1]] != null)
                            {
                                queue.Add(map[newPos[0], newPos[1]]);
                            }
                        }
                    }
                }
            }

            int requiredKeys = 0;
            foreach (Cell l in locks)
            {
                HashSet<Cell> newVisited = new HashSet<Cell>();
                foreach (Cell v in visited)
                {
                    newVisited.Add(v);
                }

                if (!CheckIsSolvable(map, l, newVisited))
                {
                    requiredKeys += 1;
                }
            }

            if (requiredKeys < keys)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generate a list of all the different permutations of the integer values in the list
        /// </summary>
        /// <param name="values">the input values used in the permutations</param>
        /// <param name="size">the size of the permutation</param>
        /// <returns>a list of all different permutations</returns>
        public static List<List<int>> GetPermutations(List<int> values, int size)
        {
            List<List<int>> result = new List<List<int>>();
            if (size == 0)
            {
                return result;
            }

            for (int i = 0; i < values.Count; i++)
            {
                List<int> clone = CopyList(values);
                clone.RemoveAt(i);
                List<List<int>> tempResult = GetPermutations(clone, size - 1);
                if (tempResult.Count == 0)
                {
                    result.Add(new List<int>());
                    result[result.Count - 1].Add(values[i]);
                }

                foreach (List<int> list in tempResult)
                {
                    list.Insert(0, values[i]);
                    result.Add(list);
                }
            }

            return result;
        }

        /// <summary>
        /// Shuffle the input list in place using the Random object
        /// </summary>
        /// <param name="random">a random object to use to shuffle the list</param>
        /// <param name="list">the list that need to be shuffled</param>
        public static void ShuffleList<T>(Random random, List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int newIndex = random.Next(list.Count);
                T temp = list[i];
                list[i] = list[newIndex];
                list[newIndex] = temp;
            }
        }
    }
}
