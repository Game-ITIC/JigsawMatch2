using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.Android;
using UnityEngine;

/// <summary>
/// Unity 6000's Gradle export asks for Build-Tools 35 and android-31.
/// This editor ships only Build-Tools 36 and platforms 34/35/36, and the
/// Unity SDK has no accepted licenses folder, so Gradle fails while trying
/// to download the missing packages. Rewrite generated Gradle files to the
/// highest already-installed SDK pieces before Gradle runs.
/// </summary>
public sealed class AndroidGradleSdkAlign : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 100;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        var gradleRoot = Directory.GetParent(path)?.FullName;
        if (string.IsNullOrEmpty(gradleRoot))
            return;

        var sdkRoot = AndroidExternalToolsSettings.sdkRootPath;
        var compileSdk = FindHighestPlatformApi(sdkRoot) ?? 36;
        var buildTools = FindHighestBuildTools(sdkRoot) ?? "36.0.0";

        foreach (var gradlePath in Directory.GetFiles(gradleRoot, "build.gradle", SearchOption.AllDirectories))
        {
            var contents = File.ReadAllText(gradlePath);
            var updated = AlignSdkVersions(contents, compileSdk, buildTools);
            if (updated == contents)
                continue;

            File.WriteAllText(gradlePath, updated);
            Debug.Log($"[AndroidGradleSdkAlign] {gradlePath} -> compileSdk {compileSdk}, buildTools {buildTools}");
        }
    }

    internal static string AlignSdkVersions(string contents, int compileSdk, string buildTools)
    {
        var updated = Regex.Replace(contents, @"compileSdkVersion\s+\d+", "compileSdkVersion " + compileSdk);
        updated = Regex.Replace(updated, @"compileSdk\s+\d+", "compileSdk " + compileSdk);
        updated = Regex.Replace(
            updated,
            @"buildToolsVersion\s*=\s*['""][^'""]+['""]",
            "buildToolsVersion = \"" + buildTools + "\"");
        updated = Regex.Replace(
            updated,
            @"buildToolsVersion\s+['""][^'""]+['""]",
            "buildToolsVersion '" + buildTools + "'");
        return updated;
    }

    static int? FindHighestPlatformApi(string sdkRoot)
    {
        var platformsDir = Path.Combine(sdkRoot ?? string.Empty, "platforms");
        if (!Directory.Exists(platformsDir))
            return null;

        var apis = Directory.GetDirectories(platformsDir)
            .Select(Path.GetFileName)
            .Where(name => name != null && name.StartsWith("android-", StringComparison.OrdinalIgnoreCase))
            .Select(name => name.Substring("android-".Length))
            .Select(value => int.TryParse(value, out var api) ? api : (int?)null)
            .Where(api => api.HasValue)
            .Select(api => api.Value)
            .ToArray();

        return apis.Length == 0 ? null : apis.Max();
    }

    static string FindHighestBuildTools(string sdkRoot)
    {
        var buildToolsDir = Path.Combine(sdkRoot ?? string.Empty, "build-tools");
        if (!Directory.Exists(buildToolsDir))
            return null;

        return Directory.GetDirectories(buildToolsDir)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name))
            .OrderByDescending(name => Version.TryParse(name, out var version) ? version : new Version(0, 0))
            .FirstOrDefault();
    }
}
