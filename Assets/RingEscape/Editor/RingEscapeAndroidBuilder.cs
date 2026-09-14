using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class RingEscapeAndroidBuilder
{
    private const string ScenePath = "Assets/RingEscape/RingEscapePrototype.unity";
    private const string VersionName = "0.1.0";

    [MenuItem("RingEscape/Build Android APK")]
    public static void BuildAndroidApk()
    {
        string androidModulePath = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines", "AndroidPlayer");
        if (!Directory.Exists(androidModulePath))
        {
            const string message = "Android Build Support is not installed for this Unity editor. Install Android Build Support, Android SDK & NDK Tools, and OpenJDK through Unity Hub, then restart Unity.";
            Debug.LogError("RingEscape Android build unavailable: " + message);
            EditorUtility.DisplayDialog("Android Build Support Required", message, "OK");
            return;
        }

        if (!File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog("RingEscape", "Build the prototype scene first.", "OK");
            return;
        }

        string outputDirectory = Path.Combine(Application.dataPath, "..", "Builds", "Android");
        Directory.CreateDirectory(outputDirectory);
        string outputPath = Path.Combine(outputDirectory, "RingEscape_Android_v" + VersionName + ".apk");

        EditorUserBuildSettings.buildAppBundle = false;
        EditorBuildSettingsScene[] configuredScenes = EditorBuildSettings.scenes;
        string[] scenes = configuredScenes.Length > 0 ? GetEnabledScenePaths(configuredScenes) : new[] { ScenePath };
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
            EditorUtility.RevealInFinder(outputPath);
        else
        {
            Debug.LogError("RingEscape Android build failed: " + report.summary.result + ". Errors: " + report.summary.totalErrors + ", warnings: " + report.summary.totalWarnings);
            for (int index = 0; index < report.steps.Length; index++)
            {
                for (int messageIndex = 0; messageIndex < report.steps[index].messages.Length; messageIndex++)
                    Debug.LogError(report.steps[index].messages[messageIndex].content);
            }
        }
    }

    private static string[] GetEnabledScenePaths(EditorBuildSettingsScene[] configuredScenes)
    {
        System.Collections.Generic.List<string> paths = new System.Collections.Generic.List<string>();
        for (int index = 0; index < configuredScenes.Length; index++)
        {
            if (configuredScenes[index].enabled)
                paths.Add(configuredScenes[index].path);
        }
        return paths.Count > 0 ? paths.ToArray() : new[] { ScenePath };
    }
}
