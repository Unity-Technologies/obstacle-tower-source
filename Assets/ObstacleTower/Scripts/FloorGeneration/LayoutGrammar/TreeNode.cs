using System;
using System.Collections.Generic;

namespace ObstacleTowerGeneration.LayoutGrammar
{
    /// <summary>
    /// Used to get a path between two nodes in the map
    /// </summary>
    class TreeNode : IComparable<TreeNode>
    {
        /// Current x location in the layout
        public int x { set; get; }

        /// Current y location in the layout
        public int y { set; get; }

        /// The parent node in the path
        public TreeNode parent { set; get; }

        /// the goal x location
        private int endX { set; get; }

        /// the goal y location
        private int endY { set; get; }

        /// <summary>
        /// Constructor for the tree node used to find path in the map layout
        /// </summary>
        /// <param name="x">the current x</param>
        /// <param name="y">the current y</param>
        /// <param name="parent">the current parent node</param>
        /// <param name="endX">the goal x</param>
        /// <param name="endY">the goal y</param>
        public TreeNode(int x, int y, TreeNode parent, int endX, int endY)
        {
            this.x = x;
            this.y = y;
            this.parent = parent;
            this.endX = endX;
            this.endY = endY;
        }

        /// <summary>
        /// Get the path from that node to the root node
        /// </summary>
        /// <returns>the x, y locations from starting cell to that cell</returns>
        public List<int[]> GetPath()
        {
            List<int[]> result = new List<int[]>();
            TreeNode current = this;
            while (current != null)
            {
                result.Add(new[] {current.x, current.y});
                current = current.parent;
            }

            result.Reverse();
            return result;
        }

        /// <summary>
        /// check which node is closer to the end location
        /// </summary>
        /// <param name="other">the other node to be compared with</param>
        /// <returns>1 if current node is further and 0 if the same and -1 otherwise</returns>
        public int CompareTo(TreeNode other)
        {
            return (Math.Abs(x - endX) + Math.Abs(y - endY)) -
                   (Math.Abs(other.x - endX) + Math.Abs(other.y - endY));
        }
    }
}
