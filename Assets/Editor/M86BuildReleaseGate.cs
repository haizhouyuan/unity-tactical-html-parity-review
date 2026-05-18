#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class M86BuildReleaseGate
{
    private const string ReportPath = "docs/M86_BUILD_RELEASE_GATE.json";
    private const string MarkdownPath = "docs/M86_BUILD_RELEASE_GATE.md";
    private const string AcceptanceReportPath = "docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json";
    private const string ScenePath = "Assets/Scenes/TacticalPrototype.unity";
    private const string BuildOutputPath = "Builds/M86/TacticalPrototypeM86.app";

    [MenuItem("AI Tools/Run M86 Build Release Gate")]
    public static void Run()
    {
        Directory.CreateDirectory("docs");
        Directory.CreateDirectory("Builds/M86");

        var acceptanceJson = ReadText(AcceptanceReportPath);
        var prerequisitesPassed = ExtractBool(acceptanceJson, "all_required_current_gates_passed")
            && ExtractBool(acceptanceJson, "m85_visual_production_passed")
            && !ExtractBool(acceptanceJson, "full_visual_asset_gate_passed");
        var sceneExists = File.Exists(ScenePath);
        var buildSettingsSceneEnabled = BuildSettingsContainsScene(ScenePath);
        var outputAbsolutePath = Path.GetFullPath(BuildOutputPath);
        CleanOutput(outputAbsolutePath);

        var buildResult = "NotRun";
        var buildSucceeded = false;
        var totalErrors = 0;
        var totalWarnings = 0;
        long buildSize = 0;
        double totalSeconds = 0.0;
        string summary = "";

        if (prerequisitesPassed && sceneExists)
        {
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = BuildOutputPath,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.Development
            };

            var report = BuildPipeline.BuildPlayer(options);
            var buildSummary = report.summary;
            buildResult = buildSummary.result.ToString();
            buildSucceeded = buildSummary.result == BuildResult.Succeeded;
            totalErrors = (int)buildSummary.totalErrors;
            totalWarnings = (int)buildSummary.totalWarnings;
            buildSize = (long)buildSummary.totalSize;
            totalSeconds = buildSummary.totalTime.TotalSeconds;
            summary = buildSummary.ToString();
        }

        var bundleExists = Directory.Exists(outputAbsolutePath);
        var executablePath = FindExecutable(outputAbsolutePath);
        var executableExists = !string.IsNullOrEmpty(executablePath) && File.Exists(executablePath);
        var fileCount = bundleExists ? Directory.GetFiles(outputAbsolutePath, "*", SearchOption.AllDirectories).Length : 0;
        var directoryBytes = bundleExists ? DirectorySize(outputAbsolutePath) : 0L;
        var passed = prerequisitesPassed
            && sceneExists
            && buildSettingsSceneEnabled
            && buildSucceeded
            && bundleExists
            && executableExists
            && fileCount > 0
            && directoryBytes > 0L;

        WriteJson(
            passed,
            prerequisitesPassed,
            sceneExists,
            buildSettingsSceneEnabled,
            BuildTarget.StandaloneOSX.ToString(),
            BuildOutputPath,
            outputAbsolutePath,
            buildResult,
            totalErrors,
            totalWarnings,
            buildSize,
            totalSeconds,
            bundleExists,
            executableExists,
            executablePath,
            fileCount,
            directoryBytes,
            summary);
        WriteMarkdown(passed, outputAbsolutePath, executablePath, buildResult, totalErrors, totalWarnings, directoryBytes);

        AssetDatabase.Refresh();
        Debug.Log("[AI Tools] M86 build release gate report written to " + ReportPath + " passed=" + passed);
    }

    private static bool BuildSettingsContainsScene(string scenePath)
    {
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene != null && scene.enabled && scene.path == scenePath)
            {
                return true;
            }
        }
        return false;
    }

    private static void CleanOutput(string absolutePath)
    {
        if (Directory.Exists(absolutePath))
        {
            Directory.Delete(absolutePath, true);
        }
        else if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
    }

    private static string FindExecutable(string appPath)
    {
        var macOsPath = Path.Combine(appPath, "Contents", "MacOS");
        if (!Directory.Exists(macOsPath))
        {
            return "";
        }

        foreach (var file in Directory.GetFiles(macOsPath))
        {
            return Path.GetFullPath(file);
        }
        return "";
    }

    private static long DirectorySize(string directory)
    {
        var total = 0L;
        foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
        {
            total += new FileInfo(file).Length;
        }
        return total;
    }

    private static string ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    private static bool ExtractBool(string json, string key)
    {
        return !string.IsNullOrEmpty(json) && Regex.IsMatch(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*true");
    }

    private static void WriteJson(
        bool passed,
        bool prerequisitesPassed,
        bool sceneExists,
        bool buildSettingsSceneEnabled,
        string buildTarget,
        string outputPath,
        string outputAbsolutePath,
        string buildResult,
        int totalErrors,
        int totalWarnings,
        long buildSize,
        double totalSeconds,
        bool bundleExists,
        bool executableExists,
        string executablePath,
        int fileCount,
        long directoryBytes,
        string summary)
    {
        var json = new StringBuilder();
        json.AppendLine("{");
        Append(json, "schema", "m86_build_release_gate_v1", true);
        Append(json, "timestamp_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
        Append(json, "passed", passed, true);
        Append(json, "prerequisites_passed", prerequisitesPassed, true);
        Append(json, "acceptance_report_path", AcceptanceReportPath, true);
        Append(json, "scene_path", ScenePath, true);
        Append(json, "scene_exists", sceneExists, true);
        Append(json, "build_settings_scene_enabled", buildSettingsSceneEnabled, true);
        Append(json, "build_target", buildTarget, true);
        Append(json, "build_output_path", outputPath, true);
        Append(json, "build_output_absolute_path", outputAbsolutePath, true);
        Append(json, "build_result", buildResult, true);
        Append(json, "build_total_errors", totalErrors, true);
        Append(json, "build_total_warnings", totalWarnings, true);
        Append(json, "build_report_size_bytes", buildSize, true);
        Append(json, "build_total_seconds", totalSeconds, true);
        Append(json, "app_bundle_exists", bundleExists, true);
        Append(json, "app_executable_exists", executableExists, true);
        Append(json, "app_executable_path", executablePath, true);
        Append(json, "app_file_count", fileCount, true);
        Append(json, "app_directory_size_bytes", directoryBytes, true);
        Append(json, "launch_smoke_tested", false, true);
        Append(json, "launch_smoke_passed", false, true);
        Append(json, "launch_smoke_log_path", "", true);
        Append(json, "notes", "Build gate writes the app bundle and release metadata. Launch smoke is recorded after the built player is executed outside the Unity Editor.", true);
        Append(json, "build_summary", summary, false);
        json.AppendLine("}");
        File.WriteAllText(ReportPath, json.ToString());
    }

    private static void WriteMarkdown(bool passed, string outputAbsolutePath, string executablePath, string buildResult, int totalErrors, int totalWarnings, long directoryBytes)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# M86 Build Release Gate");
        markdown.AppendLine();
        markdown.AppendLine("Generated: " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        markdown.AppendLine();
        markdown.AppendLine("- Passed: `" + passed + "`");
        markdown.AppendLine("- Build result: `" + buildResult + "`");
        markdown.AppendLine("- Total errors: `" + totalErrors + "`");
        markdown.AppendLine("- Total warnings: `" + totalWarnings + "`");
        markdown.AppendLine("- Build output: `" + outputAbsolutePath + "`");
        markdown.AppendLine("- Executable: `" + executablePath + "`");
        markdown.AppendLine("- Directory bytes: `" + directoryBytes + "`");
        markdown.AppendLine();
        markdown.AppendLine("The build artifact is intentionally local and ignored by Git under `Builds/`. Commit the report and release notes, not the generated app bundle.");
        File.WriteAllText(MarkdownPath, markdown.ToString());
    }

    private static void Append(StringBuilder json, string key, string value, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": \"").Append(Escape(value)).Append("\"");
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, bool value, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": ").Append(value ? "true" : "false");
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, int value, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": ").Append(value.ToString(CultureInfo.InvariantCulture));
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, long value, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": ").Append(value.ToString(CultureInfo.InvariantCulture));
        json.AppendLine(comma ? "," : "");
    }

    private static void Append(StringBuilder json, string key, double value, bool comma)
    {
        json.Append("  \"").Append(key).Append("\": ").Append(value.ToString("0.###", CultureInfo.InvariantCulture));
        json.AppendLine(comma ? "," : "");
    }

    private static string Escape(string value)
    {
        return (value ?? "")
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }
}
#endif
