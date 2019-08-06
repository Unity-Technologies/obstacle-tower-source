using UnityEditor;

namespace ObstacleTower.Editor
{
    public static class BuildUtility
    {
        private const string BaseSymbols = "UNITY_POST_PROCESSING_STACK_V2";
        private const string EvaluationSymbol = "OTCEVALUATION";

        [MenuItem("Obstacle Tower/Automated Build")]
        public static void BuildGame()
        {
            var path = EditorUtility.SaveFolderPanel("Choose Location of Builds", "", "");
            if (path == "")
            {
                return;
            }
            _makeBuild(path, BaseSymbols, BuildTarget.StandaloneWindows);
            _makeBuild(path, BaseSymbols, BuildTarget.StandaloneOSX);
            _makeBuild(path, BaseSymbols, BuildTarget.StandaloneLinux64);
        }

        private static void _makeBuild(
            string path,
            string symbols,
            BuildTarget target
        )
        {
            var levels = new[] {"Assets/ObstacleTower/Scenes/Procedural.unity"};

            var fullPath = path + "/ObstacleTower/obstacletower";

            if (target == BuildTarget.StandaloneWindows)
            {
                fullPath += ".exe";
            }

            if (target == BuildTarget.StandaloneLinux64)
            {
                fullPath += ".x86_64";
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbols);
            BuildPipeline.BuildPlayer(levels, fullPath, target, BuildOptions.None);
        }
    }
}
