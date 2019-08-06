using System.Collections.Generic;
using ObstacleTowerGeneration.MissionGraph;
using UnityEngine;
using Random = System.Random;

namespace ObstacleTowerGeneration
{
    /// <summary>
    /// A structure used to hold the result of the generation
    /// </summary>
    struct DungeonResult
    {
        /// get the mission graph
        public Graph missionGraph { set; get; }
        /// get the level layout
        public LayoutGrammar.Map layoutMap { set; get; }
        /// <summary>
        /// A structure to hold both the mission graph and layout
        /// </summary>
        /// <param name="missionGraph">the generated mission graph</param>
        /// <param name="layoutMap">the level layout</param>
        public DungeonResult(Graph missionGraph, LayoutGrammar.Map layoutMap)
        {
            this.missionGraph = missionGraph;
            this.layoutMap = layoutMap;
        }
    }

    /// <summary>
    /// Main object responsible for generation of floor layouts. 
    /// </summary>
    class Program
    {
        public static Generator CreateGenerator(string grammarPath, Random randGen)
        {
            var random = randGen;
            var mg = new Generator(random);

            var grammarListAsset = Resources.Load<TextAsset>(grammarPath + "grammarList");
            var grammarMembers = Helper.SplitLines(grammarListAsset.text);
            var patterns = new Dictionary<string, Pattern>();

            foreach (var grammarMember in grammarMembers)
            {
                var input = Resources.Load<TextAsset>(grammarPath + $"grammar/{grammarMember}/input");
                var output = Resources.Load<TextAsset>(grammarPath + $"grammar/{grammarMember}/output");

                var key = grammarMember.ToLower();
                patterns.Add(key, new Pattern(random));
                patterns[key].LoadPattern(input.text, output.text);
            }

            mg.LoadPatterns(patterns);
            return mg;
        }

        /// <summary>
        /// Generate the dungeon graph and layout
        /// </summary>
        /// <param name="totalTrials">number of trials used if any of the graph or map generation fails</param>
        /// <param name="graphTrials">number of trials to generate graph before consider it a fail</param>
        /// <param name="mapTrials">number of trials to generate the layout before consider it a fail</param>
        /// <returns>the generated graph and layout</returns>
        public static DungeonResult GenerateDungeon(int totalTrials = 100, int graphTrials = 100, int mapTrials = 100,
            int recipeLength = 1, Random randomGen = null, string recipeName = "graphRecipe")
        {
            Graph resultGraph = null;
            LayoutGrammar.Map resultMap = null;

            for (int i = 0; i < totalTrials; i++)
            {
                var grammarPath = "FloorGeneration/";
                Generator mg = CreateGenerator(grammarPath, randomGen);
                for (int j = 0; j < graphTrials; j++)
                {
                    TextAsset graphStartAsset = Resources.Load<TextAsset>(grammarPath + "graphStart");
                    TextAsset graphRecipeAsset = Resources.Load<TextAsset>(grammarPath + recipeName);
                    const int numNodes = 4;
                    resultGraph = mg.GenerateDungeonFromString(graphStartAsset.text, graphRecipeAsset.text, numNodes,
                        recipeLength);

                    if (resultGraph != null && Helper.CheckIsSolvable(resultGraph, resultGraph.nodes[0]))
                    {
                        break;
                    }

                    resultGraph = null;
                }

                if (resultGraph == null)
                {
                    continue;
                }

                LayoutGrammar.Generator lg = new LayoutGrammar.Generator(randomGen);
                for (int j = 0; j < mapTrials; j++)
                {
                    resultMap = lg.GenerateDungeon(resultGraph);
                    if (resultMap != null && Helper.CheckIsSolvable(resultMap.Get2DMap(), resultMap.GetCell(0)))
                    {
                        break;
                    }

                    resultMap = null;
                }

                if (resultMap == null)
                {
                    continue;
                }

                break;
            }

            return new DungeonResult(resultGraph, resultMap);
        }
    }
}
