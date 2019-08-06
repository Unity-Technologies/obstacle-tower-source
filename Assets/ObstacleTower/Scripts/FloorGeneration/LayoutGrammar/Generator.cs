using System;
using System.Collections.Generic;

namespace ObstacleTowerGeneration.LayoutGrammar
{
    /// <summary>
    /// The generator that tries to generate a map layout
    /// </summary>
    class Generator
    {
        /// <summary>
        /// random variable for random selection
        /// </summary>
        private Random random;

        /// <summary>
        /// Constructor of the generator class for the map layout
        /// </summary>
        /// <param name="random">the same random variable to be easy to replicate the results</param>
        public Generator(Random random)
        {
            this.random = random;
        }

        /// <summary>
        /// Generate a map layout that correspond to the input mission graph
        /// </summary>
        /// <param name="graph">the mission graph that need to be mapped to a 2D layout</param>
        /// <returns>a 2D layout of the mission graph</returns>
        public Map GenerateDungeon(MissionGraph.Graph graph)
        {
            Map result = new Map(this.random);
            result.InitializeCell(graph.nodes[0]);

            #region  make initial dungeon

            List<MissionGraph.Node> open = new List<MissionGraph.Node>();
            Dictionary<MissionGraph.Node, int> parentIDs = new Dictionary<MissionGraph.Node, int>();
            foreach (MissionGraph.Node child in graph.nodes[0].GetChildren())
            {
                open.Add(child);
                parentIDs.Add(child, 0);
            }

            HashSet<MissionGraph.Node> nodes = new HashSet<MissionGraph.Node>();
            nodes.Add(graph.nodes[0]);
            while (open.Count > 0)
            {
                MissionGraph.Node current = open[0];
                open.RemoveAt(0);
                if (nodes.Contains(current))
                {
                    continue;
                }

                nodes.Add(current);
                if (!result.AddCell(current, parentIDs[current]))
                {
                    return null;
                }

                foreach (MissionGraph.Node child in current.GetChildren())
                {
                    if (!parentIDs.ContainsKey(child))
                    {
                        if (current.type == MissionGraph.NodeType.Lock ||
                            current.type == MissionGraph.NodeType.Puzzle)
                        {
                            parentIDs.Add(child, current.id);
                        }
                        else
                        {
                            parentIDs.Add(child, parentIDs[current]);
                        }
                    }

                    open.Add(child);
                }
            }

            #endregion

            #region make lever connections

            open.Clear();
            nodes.Clear();
            open.Add(graph.nodes[0]);
            while (open.Count > 0)
            {
                MissionGraph.Node current = open[0];
                open.RemoveAt(0);
                if (nodes.Contains(current))
                {
                    continue;
                }

                nodes.Add(current);
                foreach (MissionGraph.Node child in current.GetChildren())
                {
                    Cell from = result.GetCell(current.id);
                    Cell to = result.GetCell(child.id);
                    if (current.type == MissionGraph.NodeType.Lever)
                    {
                        if (!result.MakeConnection(from, to, nodes.Count * nodes.Count))
                        {
                            return null;
                        }
                    }

                    open.Add(child);
                }
            }

            #endregion

            return result;
        }
    }
}
