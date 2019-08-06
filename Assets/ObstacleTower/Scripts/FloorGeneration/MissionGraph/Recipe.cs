using System.Collections.Generic;
using System.IO;

namespace ObstacleTowerGeneration.MissionGraph
{
    /// <summary>
    /// A class for the action of applying the patterns any number of times
    /// </summary>
    class Recipe
    {
        /// the current pattern to be applied
        public string action { set; get; }
        /// minimum number of times to apply the pattern
        public int minTimes { set; get; }
        /// the maximum number of times to apply the pattern
        public int maxTimes { set; get; }

        /// <summary>
        /// constructor for the recipe class
        /// </summary>
        /// <param name="action">the current pattern to be executed</param>
        /// <param name="minTimes">the minimum number of times to execute the pattern</param>
        /// <param name="maxTimes">the maximum number of times to execute the pattern</param>
        public Recipe(string action, int minTimes, int maxTimes)
        {
            this.action = action;
            this.minTimes = minTimes;
            this.maxTimes = maxTimes;
        }

        /// <summary>
        /// Static function to load a list of all the recipe that need to be executed from a txt file
        /// </summary>
        /// <param name="filename">the txt file that contain the recipe list</param>
        /// <returns>A list of all the recipe that need to be applied in order</returns>
        public static List<Recipe> LoadRecipes(string filename, int recipeLength)
        {
            List<Recipe> recipes;

            using (StreamReader r = new StreamReader(filename))
            {
                string[] text = Helper.SplitLines(r.ReadToEnd());
                recipes = LoadRecipes(text, recipeLength);
            }

            return recipes;
        }

        public static List<Recipe> LoadRecipes(string[] text, int recipeLength)
        {
            List<Recipe> recipes = new List<Recipe>();

            foreach (string line in text)
            {
                string[] parts = line.Split(",".ToCharArray());
                if (parts[0].Trim().ToLower() == "any")
                {
                    recipes.Add(new Recipe(parts[0].Trim(), 0, 0));
                }
                else if (parts[0].Trim()[0] == '#')
                {
                    continue;
                }
                else
                {
                    int minValue;
                    int maxValue;
                    if (parts.Length == 1)
                    {
                        minValue = 1;
                        maxValue = 1;
                    }
                    else if (parts.Length == 2)
                    {
                        minValue = int.Parse(parts[1]);
                        maxValue = minValue;
                    }
                    else
                    {
                        minValue = int.Parse(parts[1]);
                        maxValue = int.Parse(parts[2]);
                    }

                    recipes.Add(new Recipe(parts[0].Trim(), minValue, maxValue));
                }

                if (recipes.Count == recipeLength)
                {
                    break;
                }
            }

            return recipes;
        }

        public static List<Recipe> LoadRecipesFromString(string recipesString, int recipeLength)
        {
            string[] recipeStrings = Helper.SplitLines(recipesString);
            return LoadRecipes(recipeStrings, recipeLength);
        }
    }
}
