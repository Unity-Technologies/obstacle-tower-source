using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace ObstacleTower.Editor
{
    // ensure class initializer is called whenever scripts recompile
    [InitializeOnLoadAttribute]
    public static class FloorGrammarListGenerator
    {
        // register an event handler when the class is initialized
        static FloorGrammarListGenerator()
        {
            EditorApplication.playModeStateChanged += UpdateGrammarList;
        }

        private static void UpdateGrammarList(PlayModeStateChange state)
        {
            var subFolders = AssetDatabase.GetSubFolders("Assets/ObstacleTower/Resources/FloorGeneration/grammar")
                .Select(subFolder => subFolder.Split('/').Last());
            var grammarList = string.Join("\n", subFolders);
            File.WriteAllText(Application.dataPath + "/ObstacleTower/Resources/FloorGeneration/grammarList.txt", 
                grammarList);
        }
    }
}
