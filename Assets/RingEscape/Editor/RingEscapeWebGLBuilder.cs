using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class RingEscapeWebGLBuilder
{
    private const string OutputPath = "Builds/WebGL";

    public static void Build()
    {
        Directory.CreateDirectory(OutputPath);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = OutputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("RingEscape WebGL build failed: " + report.summary.result);

        UnityEngine.Debug.Log("RingEscape WebGL build complete: " + Path.GetFullPath(OutputPath));
    }

    private static string[] GetEnabledScenes()
    {
        var scenes = new System.Collections.Generic.List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            if (scene.enabled) scenes.Add(scene.path);

        if (scenes.Count == 0)
            throw new System.Exception("No enabled scenes are configured in Build Settings.");

        return scenes.ToArray();
    }
}
