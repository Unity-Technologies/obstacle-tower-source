using System.Collections.Generic;
using System.IO;
using Random = System.Random;

namespace ObstacleTowerGeneration.MissionGraph
{
    /// <summary>
    /// Generate the mission graph from the starting graph using a recipe file
    /// </summary>
    class Generator
    {
        /// Random variable that is used to help generate different random expansions from the graph every time you run
        private Random random;
        /// A dictionary for all the patterns that are loaded from the folders
        private Dictionary<string, Pattern> patterns;

        /// <summary>
        /// Constructor for the mission graph generator
        /// </summary>
        /// <param name="random">using one random variable to be able to replicate the results by fixing the seed</param>
        public Generator(Random random)
        {
            this.random = random;
            patterns = new Dictionary<string, Pattern>();
        }

        /// <summary>
        /// Load all the different patterns that can be used in the recipe file
        /// </summary>
        /// <param name="foldername">the folder that contains all the patterns</param>
        public void LoadPatterns(string foldername)
        {
            string[] folders = Directory.GetDirectories(foldername);
            foreach (string f in folders)
            {
                string key = new DirectoryInfo(f).Name.ToLower();
                patterns.Add(key, new Pattern(random));
                patterns[key].LoadPattern(f + "/");
            }
        }

        public void LoadPatterns(Dictionary<string, Pattern> newPatterns)
        {
            patterns = newPatterns;
        }

        public Graph GenerateDungeon(string startname, string recipename,
            int maxConnections = 4, int recipeLength = 1)
        {
            Graph graph = new Graph();
            graph.LoadGraph(startname);

            List<Recipe> recipes = Recipe.LoadRecipes(recipename, recipeLength);

            return GenerateDungeon(graph, recipes, maxConnections);
        }

        /// <summary>
        /// Generate the mission graph and return it
        /// </summary>
        /// <param name="startname">the starting graph which is usually a start and exist</param>
        /// <param name="recipename">the recipe file that need to be exectuted to generate the graph</param>
        /// <param name="maxConnections">the maximum number of connection any node should have (4 is the default to help the layout generation)</param>
        /// <returns>the generated mission graph</returns>
        public Graph GenerateDungeon(Graph graph, List<Recipe> recipes, int maxConnections)
        {
            foreach (Recipe r in recipes)
            {
                if (patterns.ContainsKey(r.action.ToLower()))
                {
                    int count = random.Next(r.maxTimes - r.minTimes + 1) + r.minTimes;
                    for (int i = 0; i < count; i++)
                    {
                        patterns[r.action.ToLower()].ApplyPattern(graph, maxConnections);
                    }
                }
                else
                {
                    Pattern randomPattern = null;
                    foreach (Pattern p in patterns.Values)
                    {
                        if (randomPattern == null || random.NextDouble() < 0.3)
                        {
                            randomPattern = p;
                        }
                    }

                    randomPattern.ApplyPattern(graph, maxConnections);
                }
            }

            return graph;
        }

        public Graph GenerateDungeonFromString(string startGraphString, string recipeGraphString,
            int maxConnections = 4,
            int recipeLength = 1)
        {
            Graph graph = new Graph();
            graph.LoadGraphFromString(startGraphString);
            List<Recipe> recipes = Recipe.LoadRecipesFromString(recipeGraphString, recipeLength);
            return GenerateDungeon(graph, recipes, maxConnections);
        }
    }
}
