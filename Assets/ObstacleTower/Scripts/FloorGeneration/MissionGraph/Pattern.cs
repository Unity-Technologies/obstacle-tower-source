using System.Collections.Generic;
using System;
using System.IO;

namespace ObstacleTowerGeneration.MissionGraph
{
    class Pattern
    {
        private Random random;
        private Graph patternMatch;
        private List<Graph> patternApply;

        /// <summary>
        /// Constructor for the pattern class
        /// </summary>
        /// <param name="random">the same random object used by all the patterns to be able to replicate results</param>
        public Pattern(Random random)
        {
            this.random = random;
            patternMatch = new Graph();
            patternApply = new List<Graph>();
        }

        /// <summary>
        /// Load the pattern folder where input.txt is the graph to be matched 
        /// and output files are all applicable results
        /// </summary>
        /// <param name="foldername">the folder path for the pattern matching</param>
        public void LoadPattern(string foldername)
        {
            patternMatch = new Graph();
            patternMatch.LoadGraph(foldername + "input.txt");

            patternApply = new List<Graph>();
            string[] files = Directory.GetFiles(foldername, "output.txt");
            foreach (string f in files)
            {
                Graph temp = new Graph();
                temp.LoadGraph(f);
                patternApply.Add(temp);
            }
        }

        public void LoadPattern(string input, string output)
        {
            patternMatch = new Graph();
            patternMatch.LoadGraphFromString(input);

            patternApply = new List<Graph>();
            Graph outputGraph = new Graph();
            outputGraph.LoadGraphFromString(output);
            patternApply.Add(outputGraph);
        }

        /// <summary>
        /// a fast way to check applicability of that pattern to the subgraph in that big graph
        /// </summary>
        /// <param name="graph">the full graph</param>
        /// <param name="subgraph">a subgraph from the full graph</param>
        /// <param name="maxValue">maximum number of connections from each node in the graph (set to 4 to help the layout generator later)</param>
        /// <returns>True if that pattern is applicable and false otherwise</returns>
        private bool CheckPatternApplicable(Graph graph, int maxValue = 4)
        {
            for (int i = 0; i < patternMatch.nodes.Count; i++)
            {
                foreach (Graph patternOutput in patternApply)
                {
                    if (patternMatch.nodes[i].GetChildren().Count < patternOutput.nodes[i].GetChildren().Count)
                    {
                        if (graph.GetNumConnections(graph.nodes[i]) >= maxValue)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// apply that current pattern to a random subgraph in the mission graph (inplace change)
        /// </summary>
        /// <param name="graph">the current mission graph</param>
        /// <param name="maxConnection">maximum number of connection from any specific node (to help the layout generator)</param>
        public void ApplyPattern(Graph graph, int maxConnection = 4)
        {
            List<Graph> permutations = graph.GetPermutations(patternMatch.nodes.Count);
            Helper.ShuffleList(random, permutations);
            int maxAccessLevel = graph.GetHighestAccessLevel();
            List<int> levels = new List<int>();
            for (int i = 0; i <= maxAccessLevel; i++)
            {
                levels.Add(i);
            }

            Helper.ShuffleList(random, levels);
            Graph selectedSubgraph = new Graph();
            foreach (Graph subgraph in permutations)
            {
                foreach (int level in levels)
                {
                    patternMatch.relativeAccess = level;
                    if (patternMatch.CheckSimilarity(subgraph) &&
                        CheckPatternApplicable(graph, maxConnection))
                    {
                        selectedSubgraph = subgraph;
                        break;
                    }
                }

                if (selectedSubgraph.nodes.Count > 0)
                {
                    break;
                }
            }

            if (selectedSubgraph.nodes.Count == 0)
            {
                return;
            }

            foreach (Node n in selectedSubgraph.nodes)
            {
                List<Node> children = n.GetFilteredChildren(selectedSubgraph);
                foreach (Node c in children)
                {
                    n.RemoveLinks(c);
                }
            }

            Graph selectedPattern = patternApply[random.Next(patternApply.Count)];
            for (int i = selectedSubgraph.nodes.Count; i < selectedPattern.nodes.Count; i++)
            {
                Node newNode = new Node(graph.nodes.Count, -1, selectedPattern.nodes[i].type);
                graph.nodes.Add(newNode);
                selectedSubgraph.nodes.Add(newNode);
            }

            for (int i = 0; i < selectedPattern.nodes.Count; i++)
            {
                selectedSubgraph.nodes[i]
                    .AdjustAccessLevel(patternMatch.relativeAccess + selectedPattern.nodes[i].accessLevel);
                List<Node> children = selectedPattern.nodes[i].GetChildren();
                foreach (Node c in children)
                {
                    int index = selectedPattern.GetNodeIndex(c);
                    selectedSubgraph.nodes[i].ConnectTo(selectedSubgraph.nodes[index]);
                }
            }
        }
    }
}
