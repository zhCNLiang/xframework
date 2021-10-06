using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using XLib;

public static class BuildScript
{
    private static readonly string buildFolder = "PackageAssets";
    private static readonly string buildPath = Path.Combine(buildFolder, GetPlatformName());
    private static readonly string assetRuleFile = "Assets/PackRule.asset";
    private static readonly string assetListFile = "Assets/Manifest.asset";

    [MenuItem("BuildTools/Build Bundles")]
    public static void BuildBundles()
    {
        GameUtility.DeleteFolder(buildPath);
        GameUtility.CreateFolder(buildPath);
        var packRule = GetAsset<PackRule>(assetRuleFile);
        var bundleBuilds = packRule.GetBundleBuilds();
        var assetBundleManifest = BuildPipeline.BuildAssetBundles(buildPath, bundleBuilds.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        if (assetBundleManifest == null)
        {
            return;
        }

        var manifest = GetAsset<Manifest>(assetListFile);
        var bundleRefs = new List<BundleRef>();
        var assetRefs = new List<AssetRef>();

        var bundles = assetBundleManifest.GetAllAssetBundles();
        var bundle2Ids = new Dictionary<string, int>();
        for(int index = 0; index < bundles.Length; index++)
        {
            var bundle = bundles[index];
            bundle2Ids[bundle] = index;
        }

        for (int index = 0; index < bundles.Length; index++)
        {
            var bundle = bundles[index];
            var deps = assetBundleManifest.GetAllDependencies(bundle);
            var path = Path.Combine(buildPath, bundle);
            if (File.Exists(path))
            {
                var rename = bundle;
                using (FileStream fileStream = File.OpenRead(path))
                {
                    var name = Path.GetFileNameWithoutExtension(bundle);
                    var ext = Path.GetExtension(bundle);
                    var hash = GameUtility.CalcMD5(fileStream);
                    rename = string.Format("{0}@{1}{2}", name, hash, ext);

                    bundleRefs.Add(new BundleRef() {
                        name = rename,
                        // hash = assetBundleManifest.GetAssetBundleHash(bundle).ToString(),
                        hash = hash,
                        len = fileStream.Length,
                        deps = Array.ConvertAll(deps, input => bundle2Ids[input])
                    });
                }
                GameUtility.ReName(path, rename);
            }
            else
            {
                Debug.LogError(path + " can not find.");
            }
        }

        foreach(var bundleBuild in bundleBuilds)
        {
            var bundle = bundleBuild.assetBundleName;
            var assets = bundleBuild.assetNames;
            if (assets != null)
            {
                foreach (var asset in assets)
                {
                    Debug.Log($"Build Asset [{asset}] to Bundle [{bundle}]");
                    var bundleIndex = bundleRefs.FindIndex(input => input.name.StartsWith(bundle));
                    assetRefs.Add(new AssetRef() {
                        path = asset,
                        bundle = bundleIndex
                    }); 
                }
            }
        }

        manifest.BundleRefs = bundleRefs;
        manifest.AssetRefs = assetRefs;

        var vers = Application.version.Split('.');
        if (vers.Length != 3)
        {
            Debug.LogError("version is invalid");
            return;
        };

        var appVer = string.Format("{0}.{1}", vers[0], vers[1]);
        var resVer = vers[2];

        float.TryParse(appVer, out var appVersion);
        int.TryParse(resVer, out var resVersion);

        manifest.WriteVersion(appVersion, resVersion);

        EditorUtility.SetDirty(manifest);
        AssetDatabase.SaveAssets();

        var manifestName = "Manifest";
        BuildPipeline.BuildAssetBundles(buildPath, new AssetBundleBuild[] {
            new AssetBundleBuild() {
                assetBundleName = manifestName,
                assetBundleVariant = Assets.Extension,
                assetNames = new string[] {assetListFile}
            }
        }, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

        var ver = new Version();
        ver.appVersion = appVersion;
        ver.resVersion = resVersion;

        manifestName = manifestName.ToLower() + "." + Assets.Extension;
        using (FileStream fileStream = File.OpenRead(Path.Combine(buildPath, manifestName)))
        {
            ver.manifest.name = manifestName;
            ver.manifest.len = fileStream.Length;
            ver.manifest.hash = GameUtility.CalcMD5(fileStream);
        }
        var txt = JsonUtility.ToJson(ver, true);
        GameUtility.WriteAllText(Path.Combine(buildPath, Assets.version), txt);

        foreach(var file in Directory.GetFiles(buildPath, "*.manifest"))
        {
            GameUtility.DeleteFile(file);
        }
        GameUtility.DeleteFile(Path.Combine(buildPath, "Android"));

        AssetDatabase.Refresh();
    }

    [MenuItem("BuildTools/Copy Bundle To StreamingAssetsPath")]
    public static void CopyBundleToStreamingAssets()
    {
        var bundlePath = Application.dataPath.Replace("Assets", buildFolder);
        var streamingPath = Application.streamingAssetsPath;
        GameUtility.DeleteFolder(streamingPath);
        GameUtility.CopyFolder(bundlePath, streamingPath, (int index, int total, string src, string dest) => {
            EditorUtility.DisplayProgressBar("Copy Bundle...", $"Copy {src} => {dest}", (float)index / total);
            if (index == total)
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        });
    }

    [MenuItem("BuildTools/Open PackageAssets Folder")]
    public static void OpenPackageAssetsFolder()
    {
        Application.OpenURL(buildPath);
    }

    [MenuItem("BuildTools/Open App Update Path")]
    public static void OpenAppUpdatePath()
    {
        GameUtility.CreateFolder(Assets.updatePath);
        Application.OpenURL(Assets.updatePath);
    }

    [MenuItem("BuildTools/ClearAppUpdatePath")]
    public static void ClearAppUpdatePath()
    {
        GameUtility.DeleteFolder(Assets.updatePath);
    }

    private static T GetAsset<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }
        return asset;
    }

    private static string GetPlatformName()
    {
        return GetBuildPlatform(EditorUserBuildSettings.activeBuildTarget);
    }

    private static string GetBuildPlatform(BuildTarget target)
    {
        switch(target) {
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";
#if UNITY_2017_3_OR_NEWER
            case BuildTarget.StandaloneOSX:
                return "OSX";
#else
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSXUniversal:
                return "OSX";
#endif
            default:
                return null;
        }
    }
}
