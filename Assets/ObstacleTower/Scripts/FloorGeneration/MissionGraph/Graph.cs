using System.Collections.Generic;
using System.IO;
using System;

namespace ObstacleTowerGeneration.MissionGraph
{
    /// <summary>
    /// The mission graph class
    /// </summary>
    class Graph
    {
        /// The nodes in the graph
        public List<Node> nodes { set; get; }

        /// <summary>
        /// relative access level is only used in graph matching to 
        /// modify all the nodes access level with that value
        /// </summary>
        public int relativeAccess { set; get; }

        /// <summary>
        /// constructor that creates an empty graph
        /// </summary>
        public Graph()
        {
            nodes = new List<Node>();
            relativeAccess = 0;
        }

        public void LoadGraphFromString(string contents)
        {
            Dictionary<int, Node> nodeDictionary = new Dictionary<int, Node>();
            string[] text = Helper.SplitLines(contents);
            string type = "nodes";
            foreach (string line in text)
            {
                if (line.Trim() == "nodes" || line.Trim() == "edges" || line.Trim() == "roots")
                {
                    type = line.Trim();
                    continue;
                }

                if (line.Trim().Length == 0)
                {
                    continue;
                }

                string[] parts = line.Split(",".ToCharArray());
                switch (type)
                {
                    case "nodes":
                        int id = int.Parse(parts[0]);
                        int accessLevel = int.Parse(parts[1]);
                        NodeType nodeType = (NodeType) Enum.Parse(typeof(NodeType), parts[2].Trim());
                        nodeDictionary.Add(id, new Node(id, accessLevel, nodeType));

                        break;
                    case "edges":
                        int id1 = int.Parse(parts[0]);
                        int id2 = int.Parse(parts[1]);
                        nodeDictionary[id1].ConnectTo(nodeDictionary[id2]);
                        break;
                }
            }

            foreach (Node node in nodeDictionary.Values)
            {
                nodes.Add(node);
            }
        }

        /// <summary>
        /// load a graph from a txt file that is formated in a certain way
        /// </summary>
        /// <param name="filename">the file path</param>
        public void LoadGraph(string filename)
        {
            using (StreamReader r = new StreamReader(filename))
            {
                string contents = r.ReadToEnd();
                LoadGraphFromString(contents);
            }
        }

        /// <summary>
        /// get number of connections that is connected from that specific node
        /// </summary>
        /// <param name="node">the node where the calculation starts from</param>
        /// <returns>the number of connection that are connected in a direct or indirect way to that node</returns>
        public int GetNumConnections(Node node)
        {
            int result = node.GetChildren().Count;
            List<Node> queue = new List<Node>();
            queue.Add(node);
            HashSet<Node> visited = new HashSet<Node>();
            while (queue.Count > 0)
            {
                Node current = queue[0];
                queue.Remove(current);
                if (visited.Contains(current))
                {
                    continue;
                }

                visited.Add(current);
                if (current == node)
                {
                    result += 1;
                }

                foreach (Node child in current.GetChildren())
                {
                    queue.Add(child);
                }
            }

            return result - 1;
        }

        /// <summary>
        /// get the index of the node in the node array in the graph
        /// </summary>
        /// <param name="n">the node needed to find its index</param>
        /// <returns>index of the node in the node array</returns>
        public int GetNodeIndex(Node n)
        {
            return nodes.IndexOf(n);
        }

        /// <summary>
        /// get subset of graphs of certain size. This function is used in pattern matching.
        /// </summary>
        /// <param name="size">the size of the subset graphs</param>
        /// <returns>a list of all permutations that have a specific size</returns>
        public List<Graph> GetPermutations(int size)
        {
            List<int> indeces = new List<int>();
            for (int i = 0; i < nodes.Count; i++)
            {
                indeces.Add(i);
            }

            List<List<int>> integerPermutations = Helper.GetPermutations(indeces, size);
            List<Graph> nodePermutations = new List<Graph>();
            foreach (List<int> list in integerPermutations)
            {
                Graph temp = new Graph();
                for (int i = 0; i < list.Count; i++)
                {
                    temp.nodes.Add(nodes[list[i]]);
                }

                nodePermutations.Add(temp);
            }

            return nodePermutations;
        }

        /// <summary>
        /// get the highest access level in all the nodes in the graph
        /// </summary>
        /// <returns>the highest access level in the graph</returns>
        public int GetHighestAccessLevel()
        {
            int maxValue = 0;
            foreach (Node n in nodes)
            {
                if (n.accessLevel > maxValue)
                {
                    maxValue = n.accessLevel;
                }
            }

            return maxValue;
        }

        /// <summary>
        /// check similarity between two graphs based on the similarity of nodes and connections between them
        /// </summary>
        /// <param name="graph">the other graph that the function is checking similarity towards</param>
        /// <returns>True if they are similar and false otherwise</returns>
        public bool CheckSimilarity(Graph graph)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (!nodes[i].IsNodeEqual(graph.nodes[i], relativeAccess - graph.relativeAccess))
                {
                    return false;
                }

                List<Node> pChild = nodes[i].GetFilteredChildren(this);
                List<Node> gChild = graph.nodes[i].GetFilteredChildren(graph);
                if (gChild.Count != pChild.Count)
                {
                    return false;
                }

                List<int> testing = new List<int>();
                for (int j = 0; j < pChild.Count; j++)
                {
                    int pIndex = GetNodeIndex(pChild[j]);
                    testing.Add(pIndex);
                }

                for (int j = 0; j < gChild.Count; j++)
                {
                    int gIndex = GetNodeIndex(pChild[j]);
                    if (!testing.Remove(gIndex))
                    {
                        return false;
                    }
                }

                if (testing.Count > 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// get a representation string for the current graph
        /// </summary>
        /// <returns>a string that contain all the information about that graph</returns>
        public override string ToString()
        {
            string result = "Graph:\n";
            foreach (Node n in nodes)
            {
                result += "\t" + n;
            }

            return result;
        }
    }
}
